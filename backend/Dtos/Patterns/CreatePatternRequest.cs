using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Patterns
{
    public class CreatePatternRequest
    {
        [Range(1, 200)]
        public int? Width { get; set; }

        [Range(1, 200)]
        public int? Height { get; set; }
    }
}
