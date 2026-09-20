namespace backend.Models
{
    public class PatternCell
    {
        public Guid Id { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }

        public Guid PatternId { get; set; }
        public Pattern Pattern { get; set; } = null!;

        public Guid YarnColorId { get; set; }
        public YarnColor YarnColor { get; set; } = null!;
    }
}
