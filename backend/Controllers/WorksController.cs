using backend.Dtos.Works;
using backend.Extensions;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/works")]
    public class WorksController : ControllerBase
    {
        private readonly IWorkService _workService;

        public WorksController(IWorkService workService)
        {
            _workService = workService;
        }

        [HttpGet]
        public async Task<ActionResult<List<WorkResponse>>> Get([FromQuery] WorkQueryRequest query)
        {
            return Ok(await _workService.GetAsync(User.GetUserId(), query));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<WorkResponse>> GetById(Guid id)
        {
            return Ok(await _workService.GetByIdAsync(User.GetUserId(), id));
        }

        [HttpPost]
        public async Task<ActionResult<WorkResponse>> Create(CreateWorkRequest request)
        {
            var work = await _workService.CreateAsync(User.GetUserId(), request);
            return CreatedAtAction(nameof(GetById), new { id = work.Id }, work);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<WorkResponse>> Update(Guid id, UpdateWorkRequest request)
        {
            return Ok(await _workService.UpdateAsync(User.GetUserId(), id, request));
        }

        [HttpPut("{id:guid}/move")]
        public async Task<ActionResult<WorkResponse>> Move(Guid id, MoveWorkRequest request)
        {
            return Ok(await _workService.MoveAsync(User.GetUserId(), id, request));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _workService.DeleteAsync(User.GetUserId(), id);
            return NoContent();
        }
    }
}
