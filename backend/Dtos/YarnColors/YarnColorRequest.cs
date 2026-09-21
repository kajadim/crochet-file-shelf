using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.YarnColors
{
    public class YarnColorRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"(?s)^\s*\S.*$", ErrorMessage = "Name cannot be blank.")]
        public string Name { get; set; } = null!;

        [Required]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be in #RRGGBB format.")]
        public string HexValue { get; set; } = null!;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
