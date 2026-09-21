using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class PasswordResetCodeRepository : IPasswordResetCodeRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public PasswordResetCodeRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<PasswordResetCode?> GetActiveByUserIdAsync(Guid userId) =>
            _context.PasswordResetCodes
                .Where(c => c.UserId == userId && c.UsedAt == null)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

        public async Task AddAsync(PasswordResetCode resetCode) =>
            await _context.PasswordResetCodes.AddAsync(resetCode);

        public async Task RemoveAllForUserAsync(Guid userId)
        {
            var codes = await _context.PasswordResetCodes.Where(c => c.UserId == userId).ToListAsync();
            _context.PasswordResetCodes.RemoveRange(codes);
        }

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
