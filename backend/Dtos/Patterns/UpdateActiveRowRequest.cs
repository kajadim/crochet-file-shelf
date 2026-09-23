using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Patterns
{
    public class UpdateActiveRowRequest
    {
        [Range(0, int.MaxValue)]
        public int? Row { get; set; }
    }
}
