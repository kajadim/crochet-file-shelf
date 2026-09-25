using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class WorkRepository : IWorkRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public WorkRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<List<Work>> GetByOwnerAsync(
            Guid ownerId,
            Guid? folderId,
            string? search,
            WorkType? type,
            Guid? colorId,
            VideoPlatform? platform,
            bool includeShared)
        {
            var query = includeShared
                ? _context.Works
                    .Include(w => w.Owner)
                    .Where(w => w.OwnerId == ownerId || w.Members.Any(m => m.UserId == ownerId))
                : _context.Works.Where(w => w.OwnerId == ownerId);

            if (folderId.HasValue)
            {
                query = query.Where(w => w.FolderId == folderId.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(w => w.Type == type.Value);
            }

            if (colorId.HasValue)
            {
                query = query.Where(w => w.Pattern != null && w.Pattern.Cells.Any(c => c.YarnColorId == colorId.Value));
            }

            if (platform.HasValue)
            {
                query = query.Where(w => w.VideoReference != null && w.VideoReference.Platform == platform.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = "%" + EscapeLike(search.Trim()) + "%";
                query = query.Where(w =>
                    EF.Functions.ILike(w.Name, pattern)
                    || (w.Description != null && EF.Functions.ILike(w.Description, pattern))
                    || w.Comments.Any(c => EF.Functions.ILike(c.PlainText, pattern)));
            }

            return query.OrderBy(w => w.Name).ToListAsync();
        }

        private static string EscapeLike(string value) =>
            value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        public Task<List<Work>> GetSharedWithAsync(Guid userId) =>
            _context.Works
                .Include(w => w.Owner)
                .Where(w => w.Members.Any(m => m.UserId == userId))
                .OrderBy(w => w.Name)
                .ToListAsync();

        public Task<Work?> GetByIdAsync(Guid id) =>
            _context.Works.Include(w => w.Owner).FirstOrDefaultAsync(w => w.Id == id);

        public Task<WorkMember?> GetMemberAsync(Guid workId, Guid userId) =>
            _context.WorkMembers.FirstOrDefaultAsync(m => m.WorkId == workId && m.UserId == userId);

        public Task<Dictionary<Guid, WorkPermission>> GetMemberPermissionsAsync(Guid userId, IReadOnlyCollection<Guid> workIds) =>
            _context.WorkMembers
                .Where(m => m.UserId == userId && workIds.Contains(m.WorkId))
                .ToDictionaryAsync(m => m.WorkId, m => m.Permission);

        public Task<Work?> GetByIdAsync(Guid id, Guid ownerId) =>
            _context.Works.FirstOrDefaultAsync(w => w.Id == id && w.OwnerId == ownerId);

        public async Task AddAsync(Work work) =>
            await _context.Works.AddAsync(work);

        public void Remove(Work work) =>
            _context.Works.Remove(work);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
