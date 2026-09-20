namespace backend.Models
{
    public class Work
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public WorkType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public Guid FolderId { get; set; }
        public Folder Folder { get; set; } = null!;

        public Pattern? Pattern { get; set; }
        public VideoReference? VideoReference { get; set; }
        public WorkInvitation? Invitation { get; set; }

        public ICollection<WorkComment> Comments { get; set; } = new List<WorkComment>();
        public ICollection<WorkMember> Members { get; set; } = new List<WorkMember>();
    }
}
