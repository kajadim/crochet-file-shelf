using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.Dtos.YarnColors
{
    public class YarnColorQueryRequest
    {
        [MaxLength(100)]
        public string? Search { get; set; }

        [EnumDataType(typeof(YarnColorSort))]
        public YarnColorSort? Sort { get; set; }
    }
}
