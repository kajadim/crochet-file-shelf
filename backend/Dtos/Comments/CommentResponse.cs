namespace backend.Dtos.Comments
{
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public Guid? AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;
        public string AuthorUsername { get; set; } = null!;
        public long? AuthorAvatarVersion { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
