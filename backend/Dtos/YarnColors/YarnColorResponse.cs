namespace backend.Dtos.YarnColors
{
    public class YarnColorResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string HexValue { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int WorksUsingCount { get; set; }
    }
}
