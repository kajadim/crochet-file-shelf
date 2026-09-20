namespace backend.Models
{
    public class WorkMember
    {
        public Guid Id { get; set; }
        public WorkPermission Permission { get; set; }
        public DateTime JoinedAt { get; set; }

        public Guid WorkId { get; set; }
        public Work Work { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
