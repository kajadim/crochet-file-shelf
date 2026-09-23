using backend.Dtos.Videos;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/works/{workId:guid}/video")]
    public class VideosController : ControllerBase
    {
        private readonly IVideoService _videoService;

        public VideosController(IVideoService videoService)
        {
            _videoService = videoService;
        }

        [HttpGet]
        public async Task<ActionResult<VideoResponse>> Get(Guid workId)
        {
            return Ok(await _videoService.GetAsync(User.GetUserId(), workId));
        }

        [HttpGet("status")]
        public async Task<ActionResult<VideoStatusResponse>> GetStatus(Guid workId)
        {
            return Ok(await _videoService.GetStatusAsync(User.GetUserId(), workId));
        }

        [HttpPut]
        public async Task<ActionResult<VideoResponse>> UpdateLink(Guid workId, UpdateVideoLinkRequest request)
        {
            return Ok(await _videoService.UpdateLinkAsync(User.GetUserId(), workId, request));
        }

        [HttpPut("timestamp")]
        public async Task<ActionResult<VideoResponse>> UpdateTimestamp(Guid workId, UpdateVideoTimestampRequest request)
        {
            return Ok(await _videoService.UpdateTimestampAsync(User.GetUserId(), workId, request));
        }
    }
}
