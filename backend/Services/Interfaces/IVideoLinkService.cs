using backend.Dtos.Videos;
using backend.Models;

namespace backend.Services.Interfaces
{
    public interface IVideoLinkService
    {
        Task<VideoLinkInfo> ResolveAsync(string url);
        Task<bool?> CheckAvailabilityAsync(VideoPlatform platform, string normalizedUrl);
        Task<bool?> CheckEmbeddableAsync(VideoPlatform platform, string normalizedUrl);
    }
}
