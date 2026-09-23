using backend.Models;

namespace backend.Dtos.Videos
{
    public record VideoLinkInfo(VideoPlatform Platform, string OriginalUrl, string NormalizedUrl, int? TimestampSeconds);
}
