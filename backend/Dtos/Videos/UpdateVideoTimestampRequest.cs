using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Videos
{
    public class UpdateVideoTimestampRequest
    {
        [Range(0, 359999)]
        public int? Seconds { get; set; }
    }
}
