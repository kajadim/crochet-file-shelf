using backend.Dtos.Videos;

namespace backend.Services.Interfaces
{
    public interface IVideoService
    {
        Task<VideoResponse> GetAsync(Guid userId, Guid workId);
        Task<VideoStatusResponse> GetStatusAsync(Guid userId, Guid workId);
        Task<VideoResponse> UpdateLinkAsync(Guid userId, Guid workId, UpdateVideoLinkRequest request);
        Task<VideoResponse> UpdateTimestampAsync(Guid userId, Guid workId, UpdateVideoTimestampRequest request);
    }
}
