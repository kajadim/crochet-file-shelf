using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IFolderRepository
    {
        Task<List<Folder>> GetAllByOwnerAsync(Guid ownerId);
        Task<Folder?> GetByIdAsync(Guid id, Guid ownerId);
        Task<bool> NameExistsAmongSiblingsAsync(Guid ownerId, Guid? parentFolderId, string name, Guid? excludeFolderId);
        Task<int> CountWorksInFoldersAsync(IReadOnlyCollection<Guid> folderIds);
        Task<Folder?> GetRootByNameAsync(Guid ownerId, string name);
        Task<List<Work>> GetSharedWorksInFoldersAsync(IReadOnlyCollection<Guid> folderIds);
        Task AddAsync(Folder folder);
        void Remove(Folder folder);
        Task SaveChangesAsync();
    }
}
