using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Works
{
    public class MoveWorkRequest
    {
        [Required]
        public Guid? FolderId { get; set; }
    }
}
