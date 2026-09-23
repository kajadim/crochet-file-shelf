namespace backend.Dtos.Patterns
{
    public class PatternResponse
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int CurrentRow { get; set; }
        public int CurrentColumn { get; set; }
        public int? ActiveRow { get; set; }
        public List<PatternCellResponse> Cells { get; set; } = [];
    }
}
