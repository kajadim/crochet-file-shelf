using backend.Models;

namespace backend.Services.Interfaces
{
    public interface ITokenService
    {
        (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(User user);
        string GenerateRefreshToken();
        string HashRefreshToken(string rawToken);
    }
}
