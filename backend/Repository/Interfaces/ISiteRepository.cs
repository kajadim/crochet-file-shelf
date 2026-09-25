using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface ISiteRepository
    {
        Task<SiteReference?> GetByWorkIdAsync(Guid workId);
        Task SaveChangesAsync();
    }
}
