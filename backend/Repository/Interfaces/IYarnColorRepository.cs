using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IYarnColorRepository
    {
        Task<List<YarnColor>> GetActiveByOwnerAsync(Guid ownerId);
        Task<YarnColor?> GetActiveByIdAsync(Guid id, Guid ownerId);
        Task<bool> NameExistsAsync(Guid ownerId, string name, Guid? excludeColorId);
        Task<Dictionary<Guid, int>> GetWorksUsingCountsAsync(Guid ownerId);
        Task<int> GetWorksUsingCountAsync(Guid colorId);
        Task AddAsync(YarnColor color);
        void Remove(YarnColor color);
        Task SaveChangesAsync();
    }
}
