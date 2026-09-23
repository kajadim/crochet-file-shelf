using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using backend.Models;

namespace backend.Services.Implementation
{
    public enum VideoLinkParseResult
    {
        Parsed,
        NeedsResolving,
        Unsupported,
    }

    public sealed record ParsedVideoLink(VideoPlatform Platform, string NormalizedUrl, string ExternalId, int? TimestampSeconds);

    public static class VideoLinkParser
    {
        private const int MaxTimestampSeconds = 359999;

        private static readonly Regex YouTubeId = new("^[A-Za-z0-9_-]{11}$", RegexOptions.Compiled);
        private static readonly Regex YouTubePath = new("^/(?:shorts|embed|live)/([A-Za-z0-9_-]{11})", RegexOptions.Compiled);
        private static readonly Regex TikTokPath = new(@"^/@([A-Za-z0-9._]+)/video/(\d+)", RegexOptions.Compiled);
        private static readonly Regex InstagramPath = new("^/(?:[A-Za-z0-9._]+/)?(p|reels?|tv)/([A-Za-z0-9_-]+)", RegexOptions.Compiled);
        private static readonly Regex PinterestPath = new(@"^/pin/(?:[^/]*--)?(\d+)", RegexOptions.Compiled);
        private static readonly Regex DurationText = new("^(?:(\\d+)h)?(?:(\\d+)m)?(?:(\\d+)s)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] AllowedDomains =
        [
            "youtube.com", "youtu.be", "tiktok.com", "instagram.com", "pinterest.com", "pin.it",
        ];

        public static bool IsAllowedHost(string host)
        {
            var lower = host.ToLowerInvariant();
            return AllowedDomains.Any(domain => lower == domain || lower.EndsWith("." + domain, StringComparison.Ordinal));
        }

        public static VideoLinkParseResult Parse(string input, out ParsedVideoLink? link)
        {
            link = null;

            if (!Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                return VideoLinkParseResult.Unsupported;
            }

            var host = uri.Host.ToLowerInvariant();
            var path = uri.AbsolutePath;
            var query = HttpUtility.ParseQueryString(uri.Query);

            if (host is "youtube.com" or "www.youtube.com" or "m.youtube.com")
            {
                string? id = null;
                if (path == "/watch")
                {
                    id = query["v"];
                }
                else
                {
                    var match = YouTubePath.Match(path);
                    if (match.Success)
                    {
                        id = match.Groups[1].Value;
                    }
                }
                return BuildYouTube(id, query, out link);
            }

            if (host == "youtu.be")
            {
                return BuildYouTube(path.Trim('/'), query, out link);
            }

            if (host is "tiktok.com" or "www.tiktok.com" or "m.tiktok.com")
            {
                var match = TikTokPath.Match(path);
                if (match.Success)
                {
                    var id = match.Groups[2].Value;
                    link = new ParsedVideoLink(
                        VideoPlatform.TikTok,
                        $"https://www.tiktok.com/@{match.Groups[1].Value}/video/{id}",
                        id,
                        null);
                    return VideoLinkParseResult.Parsed;
                }

                return path.StartsWith("/t/", StringComparison.Ordinal)
                    ? VideoLinkParseResult.NeedsResolving
                    : VideoLinkParseResult.Unsupported;
            }

            if (host is "vm.tiktok.com" or "vt.tiktok.com")
            {
                return VideoLinkParseResult.NeedsResolving;
            }

            if (host is "instagram.com" or "www.instagram.com")
            {
                var match = InstagramPath.Match(path);
                if (!match.Success)
                {
                    return VideoLinkParseResult.Unsupported;
                }

                var kind = match.Groups[1].Value == "reels" ? "reel" : match.Groups[1].Value;
                var code = match.Groups[2].Value;
                link = new ParsedVideoLink(VideoPlatform.Instagram, $"https://www.instagram.com/{kind}/{code}/", code, null);
                return VideoLinkParseResult.Parsed;
            }

            if (host == "pin.it")
            {
                return VideoLinkParseResult.NeedsResolving;
            }

            if (host == "pinterest.com" || host.EndsWith(".pinterest.com", StringComparison.Ordinal))
            {
                var match = PinterestPath.Match(path);
                if (!match.Success)
                {
                    return VideoLinkParseResult.Unsupported;
                }

                var id = match.Groups[1].Value;
                link = new ParsedVideoLink(VideoPlatform.Pinterest, $"https://www.pinterest.com/pin/{id}/", id, null);
                return VideoLinkParseResult.Parsed;
            }

            return VideoLinkParseResult.Unsupported;
        }

        public static string? BuildEmbedUrl(VideoPlatform platform, string normalizedUrl, int? timestampSeconds)
        {
            if (Parse(normalizedUrl, out var link) != VideoLinkParseResult.Parsed || link is null || link.Platform != platform)
            {
                return null;
            }

            return platform switch
            {
                VideoPlatform.YouTube => timestampSeconds is > 0
                    ? $"https://www.youtube.com/embed/{link.ExternalId}?start={timestampSeconds}"
                    : $"https://www.youtube.com/embed/{link.ExternalId}",
                VideoPlatform.TikTok => $"https://www.tiktok.com/embed/v2/{link.ExternalId}",
                VideoPlatform.Instagram => $"{link.NormalizedUrl}embed",
                VideoPlatform.Pinterest => $"https://assets.pinterest.com/ext/embed.html?id={link.ExternalId}",
                _ => null,
            };
        }

        private static VideoLinkParseResult BuildYouTube(
            string? id,
            System.Collections.Specialized.NameValueCollection query,
            out ParsedVideoLink? link)
        {
            link = null;
            if (id is null || !YouTubeId.IsMatch(id))
            {
                return VideoLinkParseResult.Unsupported;
            }

            var timestamp = ParseTimestamp(query["t"] ?? query["start"]);
            link = new ParsedVideoLink(VideoPlatform.YouTube, $"https://www.youtube.com/watch?v={id}", id, timestamp);
            return VideoLinkParseResult.Parsed;
        }

        private static int? ParseTimestamp(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds))
            {
                return Math.Min(seconds, MaxTimestampSeconds);
            }

            var match = DurationText.Match(value);
            if (!match.Success || !match.Groups.Cast<Group>().Skip(1).Any(g => g.Success))
            {
                return null;
            }

            long total = 0;
            total += ToLong(match.Groups[1].Value) * 3600;
            total += ToLong(match.Groups[2].Value) * 60;
            total += ToLong(match.Groups[3].Value);
            return (int)Math.Min(total, MaxTimestampSeconds);
        }

        private static long ToLong(string value) =>
            long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : 0;
    }
}
