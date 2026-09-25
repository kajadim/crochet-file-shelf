using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public UsersController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("{userId:guid}/avatar")]
        public async Task<IActionResult> GetAvatar(Guid userId)
        {
            var avatar = await _profileService.GetAvatarAsync(userId);
            if (avatar is null)
            {
                return NoContent();
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(avatar.Bytes, "image/jpeg", null, new EntityTagHeaderValue($"\"{avatar.Version}\""));
        }
    }
}
