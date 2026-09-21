using backend.Dtos.YarnColors;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/yarn-colors")]
    public class YarnColorsController : ControllerBase
    {
        private readonly IYarnColorService _yarnColorService;

        public YarnColorsController(IYarnColorService yarnColorService)
        {
            _yarnColorService = yarnColorService;
        }

        [HttpGet]
        public async Task<ActionResult<List<YarnColorResponse>>> GetAll()
        {
            return Ok(await _yarnColorService.GetAllAsync(User.GetUserId()));
        }

        [HttpPost]
        public async Task<ActionResult<YarnColorResponse>> Create(YarnColorRequest request)
        {
            var color = await _yarnColorService.CreateAsync(User.GetUserId(), request);
            return StatusCode(StatusCodes.Status201Created, color);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<YarnColorResponse>> Update(Guid id, YarnColorRequest request)
        {
            return Ok(await _yarnColorService.UpdateAsync(User.GetUserId(), id, request));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _yarnColorService.DeleteAsync(User.GetUserId(), id);
            return NoContent();
        }
    }
}
