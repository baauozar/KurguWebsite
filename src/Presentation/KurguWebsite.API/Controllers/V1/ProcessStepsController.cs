using Asp.Versioning;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Application.Features.ProcessSteps.Commands;
using KurguWebsite.Application.Features.ProcessSteps.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProcessStepsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ProcessStepsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets all active process steps, ordered for display.
        /// </summary>
        /// <returns>A list of active process steps.</returns>
        [HttpGet("active")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<ProcessStepDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            var result = await _mediator.Send(new GetActiveProcessStepsQuery());
            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a new process step. (Admin Only)
        /// </summary>
        /// <param name="command">The command containing the process step data.</param>
        /// <returns>The newly created process step.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ProcessStepDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(CreateProcessStepCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? CreatedAtAction(nameof(GetActive), result.Data) : BadRequest(result.Errors);
        }

        /// <summary>
        /// Updates an existing process step. (Admin Only)
        /// </summary>
        /// <param name="id">The ID of the process step to update.</param>
        /// <param name="command">The command containing the updated data.</param>
        /// <returns>The updated process step.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ProcessStepDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, UpdateProcessStepCommand command)
        {
            if (id != command.Id) return BadRequest("ID mismatch");
            var result = await _mediator.Send(command);
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Deletes a process step. (Admin Only)
        /// </summary>
        /// <param name="id">The ID of the process step to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteProcessStepCommand { Id = id });
            return result.Succeeded ? NoContent() : NotFound(result.Errors);
        }
    }
}