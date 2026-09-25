using backend.Dtos.Notifications;
using backend.Models;

namespace backend.Services.Interfaces
{
    public interface INotificationService
    {
        Task AddAsync(Guid recipientId, NotificationType type, Guid? workId, object payload);
        Task<NotificationListResponse> GetAsync(Guid userId);
        Task MarkReadAsync(Guid userId, Guid notificationId);
        Task MarkAllReadAsync(Guid userId);
        Task DeleteAsync(Guid userId, Guid notificationId);
    }
}
