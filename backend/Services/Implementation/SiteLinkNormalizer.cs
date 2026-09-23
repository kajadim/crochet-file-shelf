using backend.Exceptions;

namespace backend.Services.Implementation
{
    public static class SiteLinkNormalizer
    {
        public static string Normalize(string input)
        {
            var trimmed = input.Trim();
            if (trimmed.Length == 0)
            {
                throw new BadRequestException(ErrorCode.SiteLinkInvalid);
            }

            if (!trimmed.Contains("://", StringComparison.Ordinal))
            {
                trimmed = "https://" + trimmed;
            }

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                || !uri.Host.Contains('.'))
            {
                throw new BadRequestException(ErrorCode.SiteLinkInvalid);
            }

            return uri.AbsoluteUri;
        }
    }
}
