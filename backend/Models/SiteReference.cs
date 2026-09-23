namespace backend.Models
{
    public class SiteReference
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = null!;

        public Guid WorkId { get; set; }
        public Work Work { get; set; } = null!;
    }
}
