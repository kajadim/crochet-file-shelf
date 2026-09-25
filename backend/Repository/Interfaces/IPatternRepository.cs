using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IPatternRepository
    {
        Task<Pattern?> GetByWorkIdAsync(Guid workId);
        Task AddAsync(Pattern pattern);
        Task<List<PatternCell>> GetAllCellsAsync(Guid patternId);
        Task AddCellAsync(PatternCell cell);
        void RemoveCell(PatternCell cell);
        Task ApplyExpansionAsync(List<PatternCell> cells, int rowOffset, int columnOffset);
        Task SaveChangesAsync();
    }
}
