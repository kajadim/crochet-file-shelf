using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Videos
{
    public class UpdateVideoLinkRequest
    {
        [Required]
        [MaxLength(2048)]
        public string Url { get; set; } = null!;
    }
}
