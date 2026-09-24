namespace backend.Dtos.Patterns
{
    public class ImportPreviewResponse
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int ColoredCells { get; set; }
        public int SkippedCells { get; set; }
        public List<ImportPreviewColor> Colors { get; set; } = [];
        public List<ImportPreviewWarning> Warnings { get; set; } = [];
    }

    public class ImportPreviewColor
    {
        public string Hex { get; set; } = null!;
        public string? ExistingName { get; set; }
        public int Count { get; set; }
    }

    public class ImportPreviewWarning
    {
        public string Cell { get; set; } = null!;
        public string Reason { get; set; } = null!;
    }
}
