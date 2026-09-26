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
        private readonly IAccountService _accountService;

        public ProfileController(IProfileService profileService, IAccountService accountService)
        {
            _profileService = profileService;
            _accountService = accountService;
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

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request)
        {
            await _accountService.DeleteAsync(User.GetUserId(), request);
            return NoContent();
        }

        [HttpDelete("avatar")]
        public async Task<ActionResult<ProfileResponse>> RemoveAvatar()
        {
            return Ok(await _profileService.RemoveAvatarAsync(User.GetUserId()));
        }
    }
}
