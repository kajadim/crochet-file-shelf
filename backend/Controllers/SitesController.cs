using backend.Dtos.Sites;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/works/{workId:guid}/site")]
    public class SitesController : ControllerBase
    {
        private readonly ISiteService _siteService;

        public SitesController(ISiteService siteService)
        {
            _siteService = siteService;
        }

        [HttpGet]
        public async Task<ActionResult<SiteResponse>> Get(Guid workId)
        {
            return Ok(await _siteService.GetAsync(User.GetUserId(), workId));
        }

        [HttpPut]
        public async Task<ActionResult<SiteResponse>> Update(Guid workId, UpdateSiteRequest request)
        {
            return Ok(await _siteService.UpdateAsync(User.GetUserId(), workId, request));
        }
    }
}
