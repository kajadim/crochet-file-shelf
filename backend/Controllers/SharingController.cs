using backend.Dtos.Sharing;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/works/{workId:guid}/sharing")]
    public class SharingController : ControllerBase
    {
        private readonly ISharingService _sharingService;

        public SharingController(ISharingService sharingService)
        {
            _sharingService = sharingService;
        }

        [HttpGet]
        public async Task<ActionResult<SharingResponse>> Get(Guid workId)
        {
            return Ok(await _sharingService.GetAsync(User.GetUserId(), workId));
        }

        [HttpPut("invitation")]
        public async Task<ActionResult<InvitationResponse>> SetInvitation(Guid workId, SetInvitationRequest request)
        {
            return Ok(await _sharingService.SetInvitationAsync(User.GetUserId(), workId, request));
        }

        [HttpDelete("invitation")]
        public async Task<IActionResult> RevokeInvitation(Guid workId)
        {
            await _sharingService.RevokeInvitationAsync(User.GetUserId(), workId);
            return NoContent();
        }

        [HttpPut("members/{memberUserId:guid}")]
        public async Task<ActionResult<MemberResponse>> UpdateMember(Guid workId, Guid memberUserId, UpdateMemberRequest request)
        {
            return Ok(await _sharingService.UpdateMemberAsync(User.GetUserId(), workId, memberUserId, request));
        }

        [HttpDelete("members/{memberUserId:guid}")]
        public async Task<IActionResult> RemoveMember(Guid workId, Guid memberUserId)
        {
            await _sharingService.RemoveMemberAsync(User.GetUserId(), workId, memberUserId);
            return NoContent();
        }

        [HttpDelete("membership")]
        public async Task<IActionResult> Leave(Guid workId)
        {
            await _sharingService.LeaveAsync(User.GetUserId(), workId);
            return NoContent();
        }
    }
}
