using backend.Models;

namespace backend.Dtos.Videos
{
    public class VideoResponse
    {
        public VideoPlatform Platform { get; set; }
        public string OriginalUrl { get; set; } = null!;
        public string NormalizedUrl { get; set; } = null!;
        public string? EmbedUrl { get; set; }
        public int? TimestampSeconds { get; set; }
    }
}
