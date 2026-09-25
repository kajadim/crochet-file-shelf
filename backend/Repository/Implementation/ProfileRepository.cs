using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public ProfileRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<Dictionary<WorkType, int>> GetWorkCountsAsync(Guid userId) =>
            _context.Works
                .Where(w => w.OwnerId == userId)
                .GroupBy(w => w.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

        public Task<List<WorkMember>> GetMembersOfOwnedWorksAsync(Guid ownerId) =>
            _context.WorkMembers
                .Include(m => m.User)
                .Include(m => m.Work)
                .Where(m => m.Work.OwnerId == ownerId)
                .ToListAsync();

        public async Task<byte[]?> GetAvatarAsync(Guid userId) =>
            (await _context.UserAvatars.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == userId))?.Data;

        public async Task SetAvatarAsync(Guid userId, byte[] data)
        {
            var existing = await _context.UserAvatars.FirstOrDefaultAsync(a => a.UserId == userId);
            if (existing is null)
            {
                await _context.UserAvatars.AddAsync(new UserAvatar { UserId = userId, Data = data });
            }
            else
            {
                existing.Data = data;
            }
        }

        public async Task RemoveAvatarAsync(Guid userId)
        {
            var existing = await _context.UserAvatars.FirstOrDefaultAsync(a => a.UserId == userId);
            if (existing is not null)
            {
                _context.UserAvatars.Remove(existing);
            }
        }

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();

        public Task<List<WorkMember>> GetMembershipsAsync(Guid userId) =>
            _context.WorkMembers
                .Include(m => m.Work)
                .ThenInclude(w => w.Owner)
                .Where(m => m.UserId == userId)
                .ToListAsync();
    }
}
