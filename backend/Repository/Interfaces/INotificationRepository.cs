using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetLatestAsync(Guid recipientId, int take);
        Task<int> CountUnreadAsync(Guid recipientId);
        Task<Notification?> GetByIdAsync(Guid id, Guid recipientId);
        Task<List<Notification>> GetUnreadAsync(Guid recipientId);
        Task AddAsync(Notification notification);
        void Remove(Notification notification);
        Task SaveChangesAsync();
    }
}
