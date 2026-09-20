namespace backend.Models
{
    public class Pattern
    {
        public Guid Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int CurrentRow { get; set; }
        public int CurrentColumn { get; set; }

        public Guid WorkId { get; set; }
        public Work Work { get; set; } = null!;

        public ICollection<PatternCell> Cells { get; set; } = new List<PatternCell>();
    }
}
