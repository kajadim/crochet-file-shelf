namespace backend.Dtos.Comments
{
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
