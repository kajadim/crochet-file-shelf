using System.Text.Json;
using backend.Dtos.Notifications;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private const int PageSize = 50;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly INotificationRepository _notificationRepository;
        private readonly IRealtimeOutbox _outbox;

        public NotificationService(INotificationRepository notificationRepository, IRealtimeOutbox outbox)
        {
            _notificationRepository = notificationRepository;
            _outbox = outbox;
        }

        public async Task AddAsync(Guid recipientId, NotificationType type, Guid? workId, object payload)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Type = type,
                Message = JsonSerializer.Serialize(payload, JsonOptions),
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                RecipientId = recipientId,
                WorkId = workId,
            };

            await _notificationRepository.AddAsync(notification);
            _outbox.Enqueue(n => n.NotificationReceivedAsync(recipientId, ToResponse(notification)));
        }

        public async Task<NotificationListResponse> GetAsync(Guid userId)
        {
            var items = await _notificationRepository.GetLatestAsync(userId, PageSize);
            var unread = await _notificationRepository.CountUnreadAsync(userId);

            return new NotificationListResponse
            {
                Items = items.Select(ToResponse).ToList(),
                UnreadCount = unread,
            };
        }

        public async Task MarkReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await GetOwnedAsync(userId, notificationId);
            notification.IsRead = true;
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task MarkAllReadAsync(Guid userId)
        {
            var unread = await _notificationRepository.GetUnreadAsync(userId);
            foreach (var notification in unread)
            {
                notification.IsRead = true;
            }
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid userId, Guid notificationId)
        {
            var notification = await GetOwnedAsync(userId, notificationId);
            _notificationRepository.Remove(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        private async Task<Notification> GetOwnedAsync(Guid userId, Guid notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId, userId);
            return notification ?? throw new NotFoundException(ErrorCode.NotificationNotFound);
        }

        private static NotificationResponse ToResponse(Notification notification) => new()
        {
            Id = notification.Id,
            Type = notification.Type,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            WorkId = notification.WorkId,
        };
    }
}
