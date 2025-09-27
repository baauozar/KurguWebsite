using Asp.Versioning;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Features.Services.Commands;
using KurguWebsite.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ServicesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ServicesController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Searches, sorts, and paginates services.
        /// </summary>
        /// <param name="queryParams">Parameters for searching, sorting, and pagination.</param>
        /// <returns>A paginated list of services.</returns>
        // ... other usings

        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedList<ServiceDto>), StatusCodes.Status200OK)] // This is now correct
        public async Task<IActionResult> Search([FromQuery] PaginationParams queryParams)
        {
            var result = await _mediator.Send(new GetPaginatedServicesQuery { Params = queryParams });

            // --- THIS IS THE FIX ---
            // Instead of returning the whole 'result' object, return the 'Data' property.
            // This matches the `ProducesResponseType` attribute above.
            return Ok(result.Data);
        }

        /// <summary>
        /// Gets all active services.
        /// </summary>
        /// <returns>A list of active services.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<ServiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllServicesQuery());
            return Ok(result.Data);
        }

        /// <summary>
        /// Gets a specific service with its features by its SEO-friendly slug.
        /// </summary>
        /// <param name="slug">The slug of the service.</param>
        /// <returns>The requested service with its details.</returns>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetailBySlug(string slug)
        {
            var result = await _mediator.Send(new GetServiceDetailBySlugQuery { Slug = slug });
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Gets a specific service by its unique ID.
        /// </summary>
        /// <param name="id">The GUID of the service.</param>
        /// <returns>The requested service.</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetServiceByIdQuery { Id = id });
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Creates a new service. (Admin Only)
        /// </summary>
        /// <param name="command">The command containing the service data.</param>
        /// <returns>The newly created service.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(CreateServiceCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data) : BadRequest(result.Errors);
        }

        /// <summary>
        /// Updates an existing service. (Admin Only)
        /// </summary>
        /// <param name="id">The ID of the service to update.</param>
        /// <param name="command">The command containing the updated data.</param>
        /// <returns>The updated service.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, UpdateServiceCommand command)
        {
            if (id != command.Id) return BadRequest("ID mismatch");
            var result = await _mediator.Send(command);
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Deletes a service. (Admin Only)
        /// </summary>
        /// <param name="id">The ID of the service to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteServiceCommand { Id = id });
            return result.Succeeded ? NoContent() : NotFound(result.Errors);
        }
    }
}