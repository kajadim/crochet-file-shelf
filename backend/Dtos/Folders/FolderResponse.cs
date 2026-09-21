namespace backend.Dtos.Folders
{
    public class FolderResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid? ParentFolderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
