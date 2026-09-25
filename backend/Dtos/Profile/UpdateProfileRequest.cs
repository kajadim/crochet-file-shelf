using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Profile
{
    public class UpdateProfileRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"(?s)^\s*\S.*$", ErrorMessage = "First name cannot be blank.")]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [RegularExpression(@"(?s)^\s*\S.*$", ErrorMessage = "Last name cannot be blank.")]
        public string LastName { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        [RegularExpression("^[a-zA-Z0-9._-]{3,30}$", ErrorMessage = "Username must have 3-30 letters, numbers, dots, dashes or underscores.")]
        public string Username { get; set; } = null!;

        [MaxLength(500)]
        public string? Bio { get; set; }
    }
}
