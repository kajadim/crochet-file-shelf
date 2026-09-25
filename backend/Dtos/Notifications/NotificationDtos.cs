using backend.Models;

namespace backend.Dtos.Notifications
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? WorkId { get; set; }
    }

    public class NotificationListResponse
    {
        public List<NotificationResponse> Items { get; set; } = [];
        public int UnreadCount { get; set; }
    }
}
