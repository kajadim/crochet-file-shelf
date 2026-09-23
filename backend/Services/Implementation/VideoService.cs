using backend.Dtos.Videos;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoLinkService _videoLinkService;

        public VideoService(IVideoRepository videoRepository, IVideoLinkService videoLinkService)
        {
            _videoRepository = videoRepository;
            _videoLinkService = videoLinkService;
        }

        public async Task<VideoResponse> GetAsync(Guid userId, Guid workId)
        {
            var video = await GetOwnedVideoAsync(userId, workId);
            return ToResponse(video);
        }

        public async Task<VideoStatusResponse> GetStatusAsync(Guid userId, Guid workId)
        {
            var video = await GetOwnedVideoAsync(userId, workId);
            var available = await _videoLinkService.CheckAvailabilityAsync(video.Platform, video.NormalizedUrl);
            return new VideoStatusResponse { Available = available };
        }

        public async Task<VideoResponse> UpdateLinkAsync(Guid userId, Guid workId, UpdateVideoLinkRequest request)
        {
            var video = await GetOwnedVideoAsync(userId, workId);
            var link = await _videoLinkService.ResolveAsync(request.Url);

            video.Platform = link.Platform;
            video.OriginalUrl = link.OriginalUrl;
            video.NormalizedUrl = link.NormalizedUrl;
            video.TimestampSeconds = link.TimestampSeconds;
            video.Work.UpdatedAt = DateTime.UtcNow;
            await _videoRepository.SaveChangesAsync();

            return ToResponse(video);
        }

        public async Task<VideoResponse> UpdateTimestampAsync(Guid userId, Guid workId, UpdateVideoTimestampRequest request)
        {
            var video = await GetOwnedVideoAsync(userId, workId);

            video.TimestampSeconds = request.Seconds;
            video.Work.UpdatedAt = DateTime.UtcNow;
            await _videoRepository.SaveChangesAsync();

            return ToResponse(video);
        }

        private async Task<VideoReference> GetOwnedVideoAsync(Guid userId, Guid workId)
        {
            var video = await _videoRepository.GetByWorkIdAsync(workId, userId);
            return video ?? throw new NotFoundException(ErrorCode.VideoNotFound);
        }

        private static VideoResponse ToResponse(VideoReference video) => new()
        {
            Platform = video.Platform,
            OriginalUrl = video.OriginalUrl,
            NormalizedUrl = video.NormalizedUrl,
            EmbedUrl = VideoLinkParser.BuildEmbedUrl(video.Platform, video.NormalizedUrl, video.TimestampSeconds),
            TimestampSeconds = video.TimestampSeconds,
        };
    }
}
