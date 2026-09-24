using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Comments
{
    public class CommentRequest
    {
        [Required]
        [MaxLength(60000)]
        public string Text { get; set; } = null!;
    }
}
