using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.Dtos.Works
{
    public class WorkQueryRequest
    {
        public Guid? FolderId { get; set; }

        [MaxLength(200)]
        public string? Search { get; set; }

        [EnumDataType(typeof(WorkType))]
        public WorkType? Type { get; set; }

        public Guid? ColorId { get; set; }

        [EnumDataType(typeof(VideoPlatform))]
        public VideoPlatform? Platform { get; set; }
    }
}
