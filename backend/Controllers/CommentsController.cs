using backend.Dtos.Comments;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/works/{workId:guid}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CommentResponse>>> Get(Guid workId)
        {
            return Ok(await _commentService.GetAsync(User.GetUserId(), workId));
        }

        [HttpPost]
        public async Task<ActionResult<CommentResponse>> Create(Guid workId, CommentRequest request)
        {
            var comment = await _commentService.CreateAsync(User.GetUserId(), workId, request);
            return StatusCode(StatusCodes.Status201Created, comment);
        }

        [HttpPut("{commentId:guid}")]
        public async Task<ActionResult<CommentResponse>> Update(Guid workId, Guid commentId, CommentRequest request)
        {
            return Ok(await _commentService.UpdateAsync(User.GetUserId(), workId, commentId, request));
        }

        [HttpDelete("{commentId:guid}")]
        public async Task<IActionResult> Delete(Guid workId, Guid commentId)
        {
            await _commentService.DeleteAsync(User.GetUserId(), workId, commentId);
            return NoContent();
        }
    }
}
