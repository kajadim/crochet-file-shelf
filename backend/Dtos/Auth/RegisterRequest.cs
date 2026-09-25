using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Auth
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

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
    }

    public class UsernameAvailabilityResponse
    {
        public bool Available { get; set; }
    }
}
