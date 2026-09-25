using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.Dtos.Works
{
    public class CreateWorkRequest
    {
        [Required]
        [MaxLength(150)]
        [RegularExpression(@"(?s)^\s*\S.*$", ErrorMessage = "Name cannot be blank.")]
        public string Name { get; set; } = null!;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [EnumDataType(typeof(WorkType))]
        public WorkType? Type { get; set; }

        [Required]
        public Guid? FolderId { get; set; }

        [MaxLength(2048)]
        public string? Url { get; set; }

        [Range(1, 200)]
        public int? Width { get; set; }

        [Range(1, 200)]
        public int? Height { get; set; }
    }
}
