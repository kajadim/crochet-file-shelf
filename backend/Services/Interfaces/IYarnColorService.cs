using backend.Dtos.YarnColors;

namespace backend.Services.Interfaces
{
    public interface IYarnColorService
    {
        Task<List<YarnColorResponse>> GetAllAsync(Guid userId, YarnColorQueryRequest query);
        Task<List<YarnColorWorkResponse>> GetWorksAsync(Guid userId, Guid colorId);
        Task<YarnColorResponse> CreateAsync(Guid userId, YarnColorRequest request);
        Task<YarnColorResponse> UpdateAsync(Guid userId, Guid colorId, YarnColorRequest request);
        Task DeleteAsync(Guid userId, Guid colorId);
    }
}
