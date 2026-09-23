using backend.Dtos.Sites;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class SiteService : ISiteService
    {
        private readonly ISiteRepository _siteRepository;

        public SiteService(ISiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        public async Task<SiteResponse> GetAsync(Guid userId, Guid workId)
        {
            var site = await GetOwnedSiteAsync(userId, workId);
            return ToResponse(site);
        }

        public async Task<SiteResponse> UpdateAsync(Guid userId, Guid workId, UpdateSiteRequest request)
        {
            var site = await GetOwnedSiteAsync(userId, workId);

            site.Url = SiteLinkNormalizer.Normalize(request.Url);
            site.Work.UpdatedAt = DateTime.UtcNow;
            await _siteRepository.SaveChangesAsync();

            return ToResponse(site);
        }

        private async Task<SiteReference> GetOwnedSiteAsync(Guid userId, Guid workId)
        {
            var site = await _siteRepository.GetByWorkIdAsync(workId, userId);
            return site ?? throw new NotFoundException(ErrorCode.SiteNotFound);
        }

        private static SiteResponse ToResponse(SiteReference site) => new() { Url = site.Url };
    }
}
