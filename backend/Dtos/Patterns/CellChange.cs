using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Patterns
{
    public class CellChange
    {
        [Range(0, int.MaxValue)]
        public int Row { get; set; }

        [Range(0, int.MaxValue)]
        public int Column { get; set; }

        public Guid? ColorId { get; set; }
    }
}
