using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public RefreshTokenRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash) =>
            _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        public async Task AddAsync(RefreshToken refreshToken) =>
            await _context.RefreshTokens.AddAsync(refreshToken);

        public async Task RevokeAllActiveForUserAsync(Guid userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
