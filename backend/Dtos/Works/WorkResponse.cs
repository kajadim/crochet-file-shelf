using backend.Models;

namespace backend.Dtos.Works
{
    public class WorkResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public WorkType Type { get; set; }
        public Guid FolderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public WorkRole Role { get; set; }
        public string? OwnerName { get; set; }
        public bool IsShared { get; set; }
    }
}
