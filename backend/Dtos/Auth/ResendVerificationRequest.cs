using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Auth
{
    public class ResendVerificationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
