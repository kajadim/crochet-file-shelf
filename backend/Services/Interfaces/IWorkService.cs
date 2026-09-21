using backend.Dtos.Works;

namespace backend.Services.Interfaces
{
    public interface IWorkService
    {
        Task<List<WorkResponse>> GetAsync(Guid userId, Guid? folderId);
        Task<WorkResponse> GetByIdAsync(Guid userId, Guid workId);
        Task<WorkResponse> CreateAsync(Guid userId, CreateWorkRequest request);
        Task<WorkResponse> UpdateAsync(Guid userId, Guid workId, UpdateWorkRequest request);
        Task<WorkResponse> MoveAsync(Guid userId, Guid workId, MoveWorkRequest request);
        Task DeleteAsync(Guid userId, Guid workId);
    }
}
