using backend.Dtos.Patterns;
using backend.Dtos.YarnColors;

namespace backend.Services.Interfaces
{
    public interface IPatternColorService
    {
        Task<List<PatternColorResponse>> GetForWorkAsync(Guid userId, Guid workId);
        Task<YarnColorResponse> CopyToPaletteAsync(Guid userId, Guid workId, Guid colorId);
        Task MoveColorsToOwnerAsync(Guid workId, Guid ownerId, Guid? fromOwnerId = null);
    }
}
