using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<WorkComment>> GetByWorkAsync(Guid workId);
        Task<WorkComment?> GetByIdAsync(Guid commentId, Guid workId);
        Task AddAsync(WorkComment comment);
        void Remove(WorkComment comment);
        Task SaveChangesAsync();
    }
}
