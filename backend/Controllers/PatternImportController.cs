using backend.Dtos.Patterns;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/patterns/import")]
    public class PatternImportController : ControllerBase
    {
        private const long MaxUploadBytes = 3 * 1024 * 1024;

        private readonly IPatternExcelService _excelService;

        public PatternImportController(IPatternExcelService excelService)
        {
            _excelService = excelService;
        }

        [HttpPost("preview")]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<ActionResult<ImportPreviewResponse>> Preview(IFormFile file)
        {
            return Ok(await _excelService.PreviewFileAsync(User.GetUserId(), file));
        }
    }
}
