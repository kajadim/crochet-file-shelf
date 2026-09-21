using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
