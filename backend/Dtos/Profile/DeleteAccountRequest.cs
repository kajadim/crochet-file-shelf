using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Profile
{
    public class DeleteAccountRequest
    {
        [Required]
        public string Password { get; set; } = null!;
    }
}
