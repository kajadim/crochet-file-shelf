namespace backend.Models
{
    public class WorkInvitation
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public WorkPermission Permission { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid WorkId { get; set; }
        public Work Work { get; set; } = null!;
    }
}
