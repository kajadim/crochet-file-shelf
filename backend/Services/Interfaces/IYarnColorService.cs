using backend.Dtos.YarnColors;

namespace backend.Services.Interfaces
{
    public interface IYarnColorService
    {
        Task<List<YarnColorResponse>> GetAllAsync(Guid userId);
        Task<YarnColorResponse> CreateAsync(Guid userId, YarnColorRequest request);
        Task<YarnColorResponse> UpdateAsync(Guid userId, Guid colorId, YarnColorRequest request);
        Task DeleteAsync(Guid userId, Guid colorId);
    }
}
