using backend.Dtos.Sites;

namespace backend.Services.Interfaces
{
    public interface ISiteService
    {
        Task<SiteResponse> GetAsync(Guid userId, Guid workId);
        Task<SiteResponse> UpdateAsync(Guid userId, Guid workId, UpdateSiteRequest request);
    }
}
