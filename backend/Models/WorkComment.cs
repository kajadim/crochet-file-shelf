namespace backend.Models
{
    public class WorkComment
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid WorkId { get; set; }
        public Work Work { get; set; } = null!;

        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;
    }
}
