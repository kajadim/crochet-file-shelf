using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Works
{
    public class UpdateWorkRequest
    {
        [Required]
        [MaxLength(150)]
        [RegularExpression(@"(?s)^\s*\S.*$", ErrorMessage = "Name cannot be blank.")]
        public string Name { get; set; } = null!;

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}
