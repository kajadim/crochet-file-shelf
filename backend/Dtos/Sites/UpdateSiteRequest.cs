using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Sites
{
    public class UpdateSiteRequest
    {
        [Required]
        [MaxLength(2048)]
        public string Url { get; set; } = null!;
    }
}
