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
        private readonly IPatternService _patternService;

        public PatternsController(IPatternService patternService)
        {
            _patternService = patternService;
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

        [HttpPut("cells")]
        public async Task<ActionResult<PatternResponse>> SetCells(Guid workId, SetCellsRequest request)
        {
            return Ok(await _patternService.SetCellsAsync(User.GetUserId(), workId, request));
        }
    }
}
