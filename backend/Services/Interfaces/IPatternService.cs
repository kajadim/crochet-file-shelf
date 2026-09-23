using backend.Dtos.Patterns;

namespace backend.Services.Interfaces
{
    public interface IPatternService
    {
        Task<PatternResponse> GetAsync(Guid userId, Guid workId);
        Task<PatternResponse> CreateAsync(Guid userId, Guid workId, CreatePatternRequest request);
        Task<PatternResponse> UpdatePositionAsync(Guid userId, Guid workId, UpdatePositionRequest request);
        Task<PatternResponse> UpdateActiveRowAsync(Guid userId, Guid workId, UpdateActiveRowRequest request);
        Task<PatternResponse> ExpandAsync(Guid userId, Guid workId, PatternEdgesRequest request);
        Task<PatternResponse> ShrinkAsync(Guid userId, Guid workId, PatternEdgesRequest request);
        Task<PatternResponse> SetCellsAsync(Guid userId, Guid workId, SetCellsRequest request);
    }
}
