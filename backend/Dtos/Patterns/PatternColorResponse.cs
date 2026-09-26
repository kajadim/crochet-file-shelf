namespace backend.Dtos.Patterns
{
    public class PatternColorResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string HexValue { get; set; } = null!;
        public string? Notes { get; set; }
        public bool IsMine { get; set; }
    }
}
