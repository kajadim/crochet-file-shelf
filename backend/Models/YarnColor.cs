namespace backend.Models
{
    public class YarnColor
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string HexValue { get; set; } = null!;
        public string? Notes { get; set; }
        public bool IsArchived { get; set; } 
        public DateTime CreatedAt { get; set; }

        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public ICollection<PatternCell> PatternCells { get; set; } = new List<PatternCell>();
    }
}
