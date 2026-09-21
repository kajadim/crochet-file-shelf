using System.Security.Cryptography;
using System.Text;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class VerificationCodeService : IVerificationCodeService
    {
        public string GenerateCode()
        {
            var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return value.ToString("D6");
        }

        public string HashCode(string code)
        {
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
