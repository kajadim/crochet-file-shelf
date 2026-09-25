using backend.Dtos.Profile;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private const long MaxUploadBytes = 400 * 1024;

        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<ActionResult<ProfileResponse>> Get()
        {
            return Ok(await _profileService.GetAsync(User.GetUserId()));
        }

        [HttpPut]
        public async Task<ActionResult<ProfileResponse>> Update(UpdateProfileRequest request)
        {
            return Ok(await _profileService.UpdateAsync(User.GetUserId(), request));
        }

        [HttpPut("avatar")]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<ActionResult<ProfileResponse>> SetAvatar(IFormFile file)
        {
            return Ok(await _profileService.SetAvatarAsync(User.GetUserId(), file));
        }

        [HttpDelete("avatar")]
        public async Task<ActionResult<ProfileResponse>> RemoveAvatar()
        {
            return Ok(await _profileService.RemoveAvatarAsync(User.GetUserId()));
        }
    }
}
