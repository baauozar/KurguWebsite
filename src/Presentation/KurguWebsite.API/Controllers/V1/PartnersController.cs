using Asp.Versioning;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Application.Features.Partners.Commands;
using KurguWebsite.Application.Features.Partners.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PartnersController : BaseApiController
    {
        private readonly IMediator _mediator;

        public PartnersController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets all active partners, ordered by display order.
        /// </summary>
        /// <returns>A list of active partners.</returns>
        [HttpGet("active")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<PartnerDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            var result = await _mediator.Send(new GetAllPartnersQuery());
            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a new partner. (Admin Only)
        /// </summary>
        /// <param name="command">The command containing the partner's data.</param>
        /// <returns>The newly created partner.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PartnerDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(CreatePartnerCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? CreatedAtAction(nameof(GetActive), result.Data) : BadRequest(result.Errors);
        }

        /// <summary>
        /// Updates an existing partner. (Admin Only)
        /// </summary>
        /// <param name="id">The ID of the partner to update.</param>
        /// <param name="command">The command containing the updated data.</param>
        /// <returns>The updated partner.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PartnerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, UpdatePartnerCommand command)
        {
            if (id != command.Id) return BadRequest("ID mismatch");
            var result = await _mediator.Send(command);
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Deletes a partner. (Admin Only)
        /// </summary>
        /// <param name="id">The ID of the partner to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeletePartnerCommand { Id = id });
            return result.Succeeded ? NoContent() : NotFound(result.Errors);
        }
    }
}