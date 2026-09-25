using backend.Models;

namespace backend.Dtos.YarnColors
{
    public class YarnColorWorkResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public WorkType Type { get; set; }
    }
}
