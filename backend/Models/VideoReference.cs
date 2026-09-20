namespace backend.Models
{
    public class VideoReference
    {
        public Guid Id { get; set; }
        public VideoPlatform Platform { get; set; }
        public string OriginalUrl { get; set; } = null!;
        public string NormalizedUrl { get; set; } = null!;
        public int? TimestampSeconds { get; set; } // samo YouTube/TikTok (Faza 0)

        public Guid WorkId { get; set; }
        public Work Work { get; set; } = null!;
    }
}
