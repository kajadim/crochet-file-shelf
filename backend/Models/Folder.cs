namespace backend.Models
{
    public class Folder
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public Guid? ParentFolderId { get; set; }
        public Folder? ParentFolder { get; set; }
        public ICollection<Folder> Subfolders { get; set; } = new List<Folder>();

        public ICollection<Work> Works { get; set; } = new List<Work>();
    }
}
