using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IYarnColorRepository
    {
        Task<List<YarnColor>> GetActiveByOwnerAsync(Guid ownerId, string? search = null, YarnColorSort sort = YarnColorSort.NameAsc);
        Task<YarnColor?> GetActiveByIdAsync(Guid id, Guid ownerId);
        Task<YarnColor?> GetByIdAsync(Guid id);
        Task<List<YarnColor>> GetUsedInPatternAsync(Guid patternId);
        Task<bool> IsUsedInPatternAsync(Guid colorId, Guid patternId);
        Task<List<(Guid WorkId, Guid OwnerId)>> GetWorksUsingColorsOfAsync(Guid colorOwnerId);
        Task<YarnColor?> FindActiveByHexAsync(Guid ownerId, string hexValue);
        Task<bool> NameExistsAsync(Guid ownerId, string name, Guid? excludeColorId);
        Task<Dictionary<Guid, int>> GetWorksUsingCountsAsync(Guid ownerId);
        Task<int> GetWorksUsingCountAsync(Guid colorId);
        Task<List<Work>> GetAccessibleWorksUsingColorAsync(Guid colorId, Guid userId);
        Task AddAsync(YarnColor color);
        void Remove(YarnColor color);
        Task SaveChangesAsync();
    }
}
