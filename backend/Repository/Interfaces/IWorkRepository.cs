using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IWorkRepository
    {
        Task<List<Work>> GetByOwnerAsync(Guid ownerId, Guid? folderId);
        Task<Work?> GetByIdAsync(Guid id, Guid ownerId);
        Task AddAsync(Work work);
        void Remove(Work work);
        Task SaveChangesAsync();
    }
}
