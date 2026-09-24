using backend.Dtos.Patterns;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/works/{workId:guid}/pattern")]
    public class PatternsController : ControllerBase
    {
        private const long MaxUploadBytes = 3 * 1024 * 1024;

        private readonly IPatternService _patternService;
        private readonly IPatternExcelService _excelService;

        public PatternsController(IPatternService patternService, IPatternExcelService excelService)
        {
            _patternService = patternService;
            _excelService = excelService;
        }

        [HttpGet]
        public async Task<ActionResult<PatternResponse>> Get(Guid workId)
        {
            return Ok(await _patternService.GetAsync(User.GetUserId(), workId));
        }

        [HttpPost]
        public async Task<ActionResult<PatternResponse>> Create(Guid workId, CreatePatternRequest request)
        {
            var pattern = await _patternService.CreateAsync(User.GetUserId(), workId, request);
            return StatusCode(StatusCodes.Status201Created, pattern);
        }

        [HttpPut("position")]
        public async Task<ActionResult<PatternResponse>> UpdatePosition(Guid workId, UpdatePositionRequest request)
        {
            return Ok(await _patternService.UpdatePositionAsync(User.GetUserId(), workId, request));
        }

        [HttpPut("active-row")]
        public async Task<ActionResult<PatternResponse>> UpdateActiveRow(Guid workId, UpdateActiveRowRequest request)
        {
            return Ok(await _patternService.UpdateActiveRowAsync(User.GetUserId(), workId, request));
        }

        [HttpPut("expand")]
        public async Task<ActionResult<PatternResponse>> Expand(Guid workId, PatternEdgesRequest request)
        {
            return Ok(await _patternService.ExpandAsync(User.GetUserId(), workId, request));
        }

        [HttpPut("shrink")]
        public async Task<ActionResult<PatternResponse>> Shrink(Guid workId, PatternEdgesRequest request)
        {
            return Ok(await _patternService.ShrinkAsync(User.GetUserId(), workId, request));
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export(Guid workId)
        {
            var file = await _excelService.ExportAsync(User.GetUserId(), workId);
            return File(file.Content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.FileName);
        }

        [HttpPost("import/preview")]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<ActionResult<ImportPreviewResponse>> PreviewImport(Guid workId, IFormFile file)
        {
            return Ok(await _excelService.PreviewImportAsync(User.GetUserId(), workId, file));
        }

        [HttpPost("import")]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<ActionResult<PatternResponse>> Import(Guid workId, IFormFile file)
        {
            var pattern = await _excelService.ImportAsync(User.GetUserId(), workId, file);
            return StatusCode(StatusCodes.Status201Created, pattern);
        }

        [HttpPut("cells")]
        public async Task<ActionResult<PatternResponse>> SetCells(Guid workId, SetCellsRequest request)
        {
            return Ok(await _patternService.SetCellsAsync(User.GetUserId(), workId, request));
        }
    }
}
