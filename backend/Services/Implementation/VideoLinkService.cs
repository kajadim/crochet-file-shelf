using System.Text.RegularExpressions;
using backend.Dtos.Videos;
using backend.Exceptions;
using backend.Models;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class VideoLinkService : IVideoLinkService
    {
        public const string HttpClientName = "video-links";
        private const int MaxRedirects = 3;

        private static readonly Regex InstagramCopyrightBlocked = new("copyright_blocked\\\\?\"\\s*:\\s*true", RegexOptions.Compiled);

        private readonly IHttpClientFactory _httpClientFactory;

        public VideoLinkService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<VideoLinkInfo> ResolveAsync(string url)
        {
            var original = url.Trim();

            var result = VideoLinkParser.Parse(original, out var parsed);
            if (result == VideoLinkParseResult.NeedsResolving)
            {
                parsed = await FollowShortLinkAsync(original);
            }

            if (parsed is null)
            {
                throw new BadRequestException(ErrorCode.VideoLinkNotSupported);
            }

            if (await CheckAvailabilityAsync(parsed.Platform, parsed.NormalizedUrl) == false)
            {
                throw new BadRequestException(ErrorCode.VideoLinkNotWorking);
            }

            return new VideoLinkInfo(parsed.Platform, original, parsed.NormalizedUrl, parsed.TimestampSeconds);
        }

        public async Task<bool?> CheckAvailabilityAsync(VideoPlatform platform, string normalizedUrl)
        {
            string endpoint;
            switch (platform)
            {
                case VideoPlatform.YouTube:
                    endpoint = "https://www.youtube.com/oembed?format=json&url=";
                    break;
                case VideoPlatform.TikTok:
                    endpoint = "https://www.tiktok.com/oembed?url=";
                    break;
                default:
                    return null;
            }

            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                using var response = await client.GetAsync(endpoint + Uri.EscapeDataString(normalizedUrl));
                var status = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return status is 400 or 401 or 403 or 404 ? false : null;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }

        public async Task<bool?> CheckEmbeddableAsync(VideoPlatform platform, string normalizedUrl)
        {
            if (platform != VideoPlatform.Instagram)
            {
                return null;
            }

            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                using var response = await client.GetAsync(normalizedUrl.TrimEnd('/') + "/embed");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var body = await response.Content.ReadAsStringAsync();
                return !InstagramCopyrightBlocked.IsMatch(body);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }

        private async Task<ParsedVideoLink?> FollowShortLinkAsync(string startUrl)
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            var current = new Uri(startUrl.Trim());

            for (var hop = 0; hop < MaxRedirects; hop++)
            {
                Uri? next;
                try
                {
                    using var response = await client.GetAsync(current, HttpCompletionOption.ResponseHeadersRead);
                    var status = (int)response.StatusCode;
                    if (status < 300 || status > 399 || response.Headers.Location is null)
                    {
                        return null;
                    }

                    next = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(current, response.Headers.Location);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    return null;
                }

                if (next.Scheme != Uri.UriSchemeHttps || !VideoLinkParser.IsAllowedHost(next.Host))
                {
                    return null;
                }

                var result = VideoLinkParser.Parse(next.ToString(), out var parsed);
                if (result == VideoLinkParseResult.Parsed)
                {
                    return parsed;
                }
                current = next;
            }

            return null;
        }
    }
}
