using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class YarnColorRepository : IYarnColorRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public YarnColorRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<List<YarnColor>> GetActiveByOwnerAsync(Guid ownerId, string? search = null, YarnColorSort sort = YarnColorSort.NameAsc)
        {
            var query = _context.YarnColors.Where(c => c.OwnerId == ownerId && !c.IsArchived);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var textPattern = "%" + EscapeLike(term) + "%";
                var hexPattern = "%" + EscapeLike(term.TrimStart('#')) + "%";
                query = query.Where(c =>
                    EF.Functions.ILike(c.Name, textPattern, "\\")
                    || EF.Functions.ILike(c.HexValue, hexPattern, "\\")
                    || (c.Notes != null && EF.Functions.ILike(c.Notes, textPattern, "\\")));
            }

            var ordered = sort switch
            {
                YarnColorSort.NameDesc => query.OrderByDescending(c => c.Name),
                YarnColorSort.HexAsc => query.OrderBy(c => c.HexValue.ToLower()).ThenBy(c => c.Name),
                YarnColorSort.HexDesc => query.OrderByDescending(c => c.HexValue.ToLower()).ThenBy(c => c.Name),
                _ => query.OrderBy(c => c.Name),
            };

            return ordered.ToListAsync();
        }

        private static string EscapeLike(string value) =>
            value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        public Task<YarnColor?> GetActiveByIdAsync(Guid id, Guid ownerId) =>
            _context.YarnColors.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId && !c.IsArchived);

        public Task<YarnColor?> GetByIdAsync(Guid id) =>
            _context.YarnColors.FirstOrDefaultAsync(c => c.Id == id);

        public Task<List<YarnColor>> GetUsedInPatternAsync(Guid patternId) =>
            _context.PatternCells
                .Where(cell => cell.PatternId == patternId)
                .Select(cell => cell.YarnColor)
                .Distinct()
                .OrderBy(color => color.Name)
                .ToListAsync();

        public Task<bool> IsUsedInPatternAsync(Guid colorId, Guid patternId) =>
            _context.PatternCells.AnyAsync(cell => cell.PatternId == patternId && cell.YarnColorId == colorId);

        public async Task<List<(Guid WorkId, Guid OwnerId)>> GetWorksUsingColorsOfAsync(Guid colorOwnerId)
        {
            var rows = await _context.PatternCells
                .Where(cell => cell.YarnColor.OwnerId == colorOwnerId)
                .Select(cell => new { cell.Pattern.WorkId, cell.Pattern.Work.OwnerId })
                .Distinct()
                .ToListAsync();

            return rows.Select(row => (row.WorkId, row.OwnerId)).ToList();
        }

        public Task<YarnColor?> FindActiveByHexAsync(Guid ownerId, string hexValue)
        {
            var hex = hexValue.ToUpper();
            return _context.YarnColors.FirstOrDefaultAsync(c => c.OwnerId == ownerId && !c.IsArchived && c.HexValue.ToUpper() == hex);
        }

        public Task<bool> NameExistsAsync(Guid ownerId, string name, Guid? excludeColorId)
        {
            var lowerName = name.ToLower();

            return _context.YarnColors.AnyAsync(c =>
                c.OwnerId == ownerId
                && !c.IsArchived
                && c.Name.ToLower() == lowerName
                && (excludeColorId == null || c.Id != excludeColorId));
        }

        public Task<Dictionary<Guid, int>> GetWorksUsingCountsAsync(Guid ownerId) =>
            _context.PatternCells
                .Where(cell => cell.YarnColor.OwnerId == ownerId
                    && (cell.Pattern.Work.OwnerId == ownerId || cell.Pattern.Work.Members.Any(m => m.UserId == ownerId)))
                .GroupBy(cell => cell.YarnColorId)
                .Select(group => new { ColorId = group.Key, Count = group.Select(cell => cell.PatternId).Distinct().Count() })
                .ToDictionaryAsync(item => item.ColorId, item => item.Count);

        public Task<int> GetWorksUsingCountAsync(Guid colorId) =>
            _context.PatternCells
                .Where(cell => cell.YarnColorId == colorId)
                .Select(cell => cell.PatternId)
                .Distinct()
                .CountAsync();

        public Task<List<Work>> GetAccessibleWorksUsingColorAsync(Guid colorId, Guid userId) =>
            _context.Works
                .Where(work => work.Pattern != null
                    && work.Pattern.Cells.Any(cell => cell.YarnColorId == colorId)
                    && (work.OwnerId == userId || work.Members.Any(m => m.UserId == userId)))
                .OrderBy(work => work.Name)
                .ToListAsync();

        public async Task AddAsync(YarnColor color) =>
            await _context.YarnColors.AddAsync(color);

        public void Remove(YarnColor color) =>
            _context.YarnColors.Remove(color);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
