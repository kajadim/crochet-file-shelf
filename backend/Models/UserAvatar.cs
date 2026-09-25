namespace backend.Models
{
    public class UserAvatar
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public byte[] Data { get; set; } = null!;
    }
}
