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

        private readonly IWorkAccessService _access;

        public SiteService(ISiteRepository siteRepository, IWorkAccessService access)
        {
            _siteRepository = siteRepository;
            _access = access;
        }

        public async Task<SiteResponse> GetAsync(Guid userId, Guid workId)
        {
            var site = await GetSiteAsync(userId, workId, WorkAccessLevel.Read);
            return ToResponse(site);
        }

        public async Task<SiteResponse> UpdateAsync(Guid userId, Guid workId, UpdateSiteRequest request)
        {
            var site = await GetSiteAsync(userId, workId, WorkAccessLevel.Owner);

            site.Url = SiteLinkNormalizer.Normalize(request.Url);
            site.Work.UpdatedAt = DateTime.UtcNow;
            await _siteRepository.SaveChangesAsync();

            return ToResponse(site);
        }

        private async Task<SiteReference> GetSiteAsync(Guid userId, Guid workId, WorkAccessLevel level)
        {
            await _access.RequireAsync(userId, workId, level);
            var site = await _siteRepository.GetByWorkIdAsync(workId);
            return site ?? throw new NotFoundException(ErrorCode.SiteNotFound);
        }

        private static SiteResponse ToResponse(SiteReference site) => new() { Url = site.Url };
    }
}
