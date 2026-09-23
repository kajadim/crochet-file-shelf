using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IVideoRepository
    {
        Task<VideoReference?> GetByWorkIdAsync(Guid workId, Guid ownerId);
        Task SaveChangesAsync();
    }
}
