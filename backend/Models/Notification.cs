namespace backend.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid RecipientId { get; set; }
        public User Recipient { get; set; } = null!;

        public Guid? WorkId { get; set; }
        public Work? Work { get; set; }
    }
}
