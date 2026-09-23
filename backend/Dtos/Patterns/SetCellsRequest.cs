using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Patterns
{
    public class SetCellsRequest
    {
        [Required]
        [MinLength(1)]
        public List<CellChange> Cells { get; set; } = [];
    }
}
