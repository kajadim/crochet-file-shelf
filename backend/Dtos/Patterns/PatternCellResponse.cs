namespace backend.Dtos.Patterns
{
    public class PatternCellResponse
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public Guid ColorId { get; set; }
        public string HexValue { get; set; } = null!;
    }
}
