using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Patterns
{
    public class PatternEdgesRequest
    {
        [Range(0, 200)]
        public int Top { get; set; }

        [Range(0, 200)]
        public int Bottom { get; set; }

        [Range(0, 200)]
        public int Left { get; set; }

        [Range(0, 200)]
        public int Right { get; set; }
    }
}
