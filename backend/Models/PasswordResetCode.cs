namespace backend.Models
{
    public class PasswordResetCode
    {
        public Guid Id { get; set; }
        public string CodeHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
