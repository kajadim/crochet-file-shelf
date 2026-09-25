using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class PatternRepository : IPatternRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public PatternRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<Pattern?> GetByWorkIdAsync(Guid workId) =>
            _context.Patterns.FirstOrDefaultAsync(p => p.WorkId == workId);

        public async Task AddAsync(Pattern pattern) =>
            await _context.Patterns.AddAsync(pattern);

        public Task<List<PatternCell>> GetAllCellsAsync(Guid patternId) =>
            _context.PatternCells
                .Include(c => c.YarnColor)
                .Where(c => c.PatternId == patternId)
                .ToListAsync();

        public async Task AddCellAsync(PatternCell cell) =>
            await _context.PatternCells.AddAsync(cell);

        public void RemoveCell(PatternCell cell) =>
            _context.PatternCells.Remove(cell);

        public async Task ApplyExpansionAsync(List<PatternCell> cells, int rowOffset, int columnOffset)
        {
            if (cells.Count == 0 || (rowOffset == 0 && columnOffset == 0))
            {
                await _context.SaveChangesAsync();
                return;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var originals = cells.Select(c => (c.RowIndex, c.ColumnIndex)).ToList();

            for (var i = 0; i < cells.Count; i++)
            {
                cells[i].RowIndex = -(originals[i].RowIndex + 1);
                cells[i].ColumnIndex = -(originals[i].ColumnIndex + 1);
            }
            await _context.SaveChangesAsync();

            for (var i = 0; i < cells.Count; i++)
            {
                cells[i].RowIndex = originals[i].RowIndex + rowOffset;
                cells[i].ColumnIndex = originals[i].ColumnIndex + columnOffset;
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
