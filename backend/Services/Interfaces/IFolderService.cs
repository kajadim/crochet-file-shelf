using backend.Dtos.Folders;

namespace backend.Services.Interfaces
{
    public interface IFolderService
    {
        Task<List<FolderResponse>> GetAllAsync(Guid userId);
        Task<FolderResponse> CreateAsync(Guid userId, CreateFolderRequest request);
        Task<FolderResponse> RenameAsync(Guid userId, Guid folderId, RenameFolderRequest request);
        Task<FolderDeletionSummary> GetDeletionSummaryAsync(Guid userId, Guid folderId);
        Task DeleteAsync(Guid userId, Guid folderId);
    }
}
