using Asp.Versioning;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Features.Services.Commands;
using KurguWebsite.Application.Features.Services.Queries;
using KurguWebsite.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    /// <summary>
    /// Manage Service Features (create, read, update, delete).
    /// Versioned route: <c>api/v1/ServiceFeatures</c>.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class ServiceFeaturesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ServiceFeaturesController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Get a single Service Feature by its identifier.
        /// </summary>
        /// <param name="id">Service Feature id (GUID).</param>
        /// <returns>Service Feature details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<ServiceFeatureDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return HandleResult(await _mediator.Send(new GetServiceFeatureByIdQuery { Id = id }));
        }

        /// <summary>
        /// Get all Service Features that belong to a specific Service.
        /// </summary>
        /// <param name="serviceId">Service id (GUID).</param>
        /// <returns>List of Service Features for the given Service.</returns>
        [HttpGet("service/{serviceId}")]
        [ProducesResponseType(typeof(Result<List<ServiceFeatureDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByServiceId(Guid serviceId)
        {
            return HandleResult(await _mediator.Send(new GetServiceFeaturesByServiceIdQuery { ServiceId = serviceId }));
        }

        /// <summary>
        /// Create a new Service Feature.
        /// </summary>
        /// <param name="command">Create command payload.</param>
        /// <returns>Created Service Feature.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Result<ServiceFeatureDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateServiceFeatureCommand command)
        {
            return HandleResult(await _mediator.Send(command));
        }

        /// <summary>
        /// Update an existing Service Feature.
        /// </summary>
        /// <param name="id">Service Feature id (GUID).</param>
        /// <param name="dto">Fields to update.</param>
        /// <returns>Updated Service Feature.</returns>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Result<ServiceFeatureDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceFeatureDto dto)
        {
            var command = new UpdateServiceFeatureCommand { Id = id, UpdateServiceFeatureDto = dto };
            return HandleResult(await _mediator.Send(command));
        }

        /// <summary>
        /// Delete a Service Feature by its identifier.
        /// </summary>
        /// <param name="id">Service Feature id (GUID).</param>
        /// <returns>Deletion status.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Result<ControlResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Your handler returns a plain ControlResult, so we wrap it into Result<ControlResult>.
            ControlResult data = await _mediator.Send(new DeleteServiceFeatureCommand { Id = id });

            if (data is null)
                return HandleResult(Result<ControlResult>.NotFound(nameof(ServiceFeature), id));

            return HandleResult(Result<ControlResult>.Success(data, "Deleted successfully"));
        }

        /// <summary>
        /// Get all Service Features.
        /// </summary>
        /// <returns>All Service Features.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ServiceFeatureDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllServiceFeaturesQuery(), ct);
            return Ok(result.Data);
        }

        /// <summary>
        /// Get Service Features with pagination and optional filtering by Service.
        /// </summary>
        /// <param name="serviceId">Optional Service id to filter by.</param>
        /// <param name="paging">Pagination parameters.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Paged Service Features.</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PaginatedList<ServiceFeatureDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? serviceId, [FromQuery] PaginationParams paging, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetServiceFeaturesPagedQuery
            {
                ServiceId = serviceId,
                Paging = paging
            }, ct);

            return Ok(result.Data);
        }
    }
}
