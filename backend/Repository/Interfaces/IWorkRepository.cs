using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IWorkRepository
    {
        Task<List<Work>> GetByOwnerAsync(
            Guid ownerId,
            Guid? folderId,
            string? search,
            WorkType? type,
            Guid? colorId,
            VideoPlatform? platform,
            bool includeShared);
        Task<List<Work>> GetSharedWithAsync(Guid userId, string? search, WorkType? type, Guid? colorId, VideoPlatform? platform);
        Task<Work?> GetByIdAsync(Guid id);
        Task<WorkMember?> GetMemberAsync(Guid workId, Guid userId);
        Task<Dictionary<Guid, WorkPermission>> GetMemberPermissionsAsync(Guid userId, IReadOnlyCollection<Guid> workIds);
        Task<Work?> GetByIdAsync(Guid id, Guid ownerId);
        Task AddAsync(Work work);
        void Remove(Work work);
        Task SaveChangesAsync();
    }
}
