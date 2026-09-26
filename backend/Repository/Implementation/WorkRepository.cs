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
            bool includeShared,
            bool? isShared)
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

            if (colorId.HasValue)
            {
                query = query.Where(w => w.Pattern != null && w.Pattern.Cells.Any(c => c.YarnColorId == colorId.Value));
            }

            if (isShared.HasValue)
            {
                query = isShared.Value ? query.Where(w => w.Members.Any()) : query.Where(w => !w.Members.Any());
            }

            return ApplyFilters(query, search, type, platform).OrderBy(w => w.Name).ToListAsync();
        }

        private static IQueryable<Work> ApplyFilters(IQueryable<Work> query, string? search, WorkType? type, VideoPlatform? platform)
        {
            if (type.HasValue)
            {
                query = query.Where(w => w.Type == type.Value);
            }

            if (platform.HasValue)
            {
                query = query.Where(w => w.VideoReference != null && w.VideoReference.Platform == platform.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = "%" + EscapeLike(search.Trim()) + "%";
                query = query.Where(w =>
                    EF.Functions.ILike(w.Name, pattern, "\\")
                    || (w.Description != null && EF.Functions.ILike(w.Description, pattern, "\\"))
                    || w.Comments.Any(c => EF.Functions.ILike(c.PlainText, pattern, "\\")));
            }

            return query;
        }

        private static string EscapeLike(string value) =>
            value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        public async Task<List<Work>> GetSharedWithAsync(
            Guid userId,
            string? search,
            WorkType? type,
            Guid? colorId,
            VideoPlatform? platform)
        {
            var query = _context.Works
                .Include(w => w.Owner)
                .Where(w => w.Members.Any(m => m.UserId == userId));

            if (colorId.HasValue)
            {
                var hex = await _context.YarnColors
                    .Where(c => c.Id == colorId.Value && c.OwnerId == userId)
                    .Select(c => c.HexValue.ToLower())
                    .FirstOrDefaultAsync();

                if (hex is null)
                {
                    return [];
                }

                query = query.Where(w => w.Pattern != null && w.Pattern.Cells.Any(c => c.YarnColor.HexValue.ToLower() == hex));
            }

            return await ApplyFilters(query, search, type, platform).OrderBy(w => w.Name).ToListAsync();
        }

        public Task<Work?> GetByIdAsync(Guid id) =>
            _context.Works.Include(w => w.Owner).FirstOrDefaultAsync(w => w.Id == id);

        public Task<List<Guid>> GetOwnedIdsAsync(Guid ownerId) =>
            _context.Works.Where(w => w.OwnerId == ownerId).Select(w => w.Id).ToListAsync();

        public Task<List<Guid>> GetMemberWorkIdsAsync(Guid userId) =>
            _context.WorkMembers.Where(m => m.UserId == userId).Select(m => m.WorkId).ToListAsync();

        public Task<WorkMember?> GetMemberAsync(Guid workId, Guid userId) =>
            _context.WorkMembers.FirstOrDefaultAsync(m => m.WorkId == workId && m.UserId == userId);

        public async Task<HashSet<Guid>> GetSharedWorkIdsAsync(IReadOnlyCollection<Guid> workIds) =>
            (await _context.WorkMembers
                .Where(m => workIds.Contains(m.WorkId))
                .Select(m => m.WorkId)
                .Distinct()
                .ToListAsync())
            .ToHashSet();

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
