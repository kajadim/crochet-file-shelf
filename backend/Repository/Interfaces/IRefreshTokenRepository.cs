using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
        Task AddAsync(RefreshToken refreshToken);
        Task RevokeAllActiveForUserAsync(Guid userId);
        Task SaveChangesAsync();
    }
}
