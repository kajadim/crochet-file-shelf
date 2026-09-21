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
        public string DisplayName { get; set; } = null!;
    }
}
