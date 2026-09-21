using backend.Dtos.Folders;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/folders")]
    public class FoldersController : ControllerBase
    {
        private readonly IFolderService _folderService;

        public FoldersController(IFolderService folderService)
        {
            _folderService = folderService;
        }

        [HttpGet]
        public async Task<ActionResult<List<FolderResponse>>> GetAll()
        {
            return Ok(await _folderService.GetAllAsync(User.GetUserId()));
        }

        [HttpPost]
        public async Task<ActionResult<FolderResponse>> Create(CreateFolderRequest request)
        {
            var folder = await _folderService.CreateAsync(User.GetUserId(), request);
            return StatusCode(StatusCodes.Status201Created, folder);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FolderResponse>> Rename(Guid id, RenameFolderRequest request)
        {
            return Ok(await _folderService.RenameAsync(User.GetUserId(), id, request));
        }

        [HttpGet("{id:guid}/deletion-summary")]
        public async Task<ActionResult<FolderDeletionSummary>> GetDeletionSummary(Guid id)
        {
            return Ok(await _folderService.GetDeletionSummaryAsync(User.GetUserId(), id));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _folderService.DeleteAsync(User.GetUserId(), id);
            return NoContent();
        }
    }
}
