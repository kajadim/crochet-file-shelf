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
            bool includeShared,
            bool? isShared);
        Task<List<Work>> GetSharedWithAsync(Guid userId, string? search, WorkType? type, Guid? colorId, VideoPlatform? platform);
        Task<Work?> GetByIdAsync(Guid id);
        Task<List<Guid>> GetOwnedIdsAsync(Guid ownerId);
        Task<List<Guid>> GetMemberWorkIdsAsync(Guid userId);
        Task<WorkMember?> GetMemberAsync(Guid workId, Guid userId);
        Task<HashSet<Guid>> GetSharedWorkIdsAsync(IReadOnlyCollection<Guid> workIds);
        Task<Dictionary<Guid, WorkPermission>> GetMemberPermissionsAsync(Guid userId, IReadOnlyCollection<Guid> workIds);
        Task<Work?> GetByIdAsync(Guid id, Guid ownerId);
        Task AddAsync(Work work);
        void Remove(Work work);
        Task SaveChangesAsync();
    }
}
