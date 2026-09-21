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

        public Task<List<YarnColor>> GetActiveByOwnerAsync(Guid ownerId) =>
            _context.YarnColors
                .Where(c => c.OwnerId == ownerId && !c.IsArchived)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public Task<YarnColor?> GetActiveByIdAsync(Guid id, Guid ownerId) =>
            _context.YarnColors.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId && !c.IsArchived);

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
                .Where(cell => cell.YarnColor.OwnerId == ownerId)
                .GroupBy(cell => cell.YarnColorId)
                .Select(group => new { ColorId = group.Key, Count = group.Select(cell => cell.PatternId).Distinct().Count() })
                .ToDictionaryAsync(item => item.ColorId, item => item.Count);

        public Task<int> GetWorksUsingCountAsync(Guid colorId) =>
            _context.PatternCells
                .Where(cell => cell.YarnColorId == colorId)
                .Select(cell => cell.PatternId)
                .Distinct()
                .CountAsync();

        public async Task AddAsync(YarnColor color) =>
            await _context.YarnColors.AddAsync(color);

        public void Remove(YarnColor color) =>
            _context.YarnColors.Remove(color);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
