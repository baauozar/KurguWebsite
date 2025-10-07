using Asp.Versioning;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Features.Services.Commands;
using KurguWebsite.Application.Features.Services.Queries;
using KurguWebsite.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    /// <summary>
    /// Manages services offered by the company
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ServicesController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(IMediator mediator, ILogger<ServicesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region Public Endpoints

        /// <summary>
        /// Gets all active services
        /// </summary>
        /// <returns>A list of active services</returns>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<ServiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllServicesQuery());
            return HandleResult(result);
        }

        /// <summary>
        /// Gets featured services
        /// </summary>
        /// <returns>A list of featured services</returns>
        [HttpGet("featured")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<ServiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeatured()
        {
            var result = await _mediator.Send(new GetFeaturedServicesQuery());
            return HandleResult(result);
        }

        /// <summary>
        /// Gets services by category
        /// </summary>
        /// <param name="category">The service category</param>
        /// <returns>A list of services in the specified category</returns>
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<ServiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCategory(ServiceCategory category)
        {
            var result = await _mediator.Send(new GetServicesByCategoryQuery { Category = category });
            return HandleResult(result);
        }

        /// <summary>
        /// Gets services that have case studies
        /// </summary>
        /// <returns>A list of services with case studies</returns>
        [HttpGet("with-case-studies")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<ServiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWithCaseStudies()
        {
            var result = await _mediator.Send(new GetServicesWithCaseStudiesQuery());
            return HandleResult(result);
        }

        /// <summary>
        /// Searches services with pagination and filtering
        /// </summary>
        /// <param name="query">Search parameters</param>
        /// <returns>A paginated list of services</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedList<ServiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] SearchServicesQuery query)
        {
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a paginated list of services with advanced filtering
        /// </summary>
        /// <param name="paginationParams">Pagination and filtering parameters</param>
        /// <returns>A paginated list of services</returns>
        [HttpGet("paginated")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedList<ServiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated([FromQuery] PaginationParams paginationParams)
        {
            var result = await _mediator.Send(new GetPaginatedServicesQuery { Params = paginationParams });
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a specific service by ID
        /// </summary>
        /// <param name="id">The service ID</param>
        /// <returns>The requested service</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetServiceByIdQuery { Id = id });
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a service with its features by ID
        /// </summary>
        /// <param name="id">The service ID</param>
        /// <returns>The service with all its features</returns>
        [HttpGet("{id:guid}/details")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWithFeatures(Guid id)
        {
            var result = await _mediator.Send(new GetServiceWithFeaturesQuery { Id = id });
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a specific service by slug
        /// </summary>
        /// <param name="slug">The service slug</param>
        /// <returns>The requested service</returns>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var result = await _mediator.Send(new GetServiceBySlugQuery { Slug = slug });
            return HandleResult(result);
        }

        #endregion

        #region Admin Endpoints

        /// <summary>
        /// Creates a new service
        /// </summary>
        /// <param name="command">The service data</param>
        /// <returns>The created service</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateServiceCommand command)
        {
            _logger.LogInformation("Creating new service: {Title}", command.Title);

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create service: {Errors}", string.Join(", ", result.Errors));
                return HandleResult(result);
            }

            _logger.LogInformation("Service created successfully with ID: {Id}", result.Data?.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result.Data);
        }

        /// <summary>
        /// Updates an existing service
        /// </summary>
        /// <param name="id">The service ID</param>
        /// <param name="command">The updated service data</param>
        /// <returns>The updated service</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceCommand command)
        {
            if (id != command.Id)
                return BadRequest(new ProblemDetails { Detail = "ID mismatch between route and body" });

            _logger.LogInformation("Updating service: {Id}", id);

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                _logger.LogWarning("Failed to update service {Id}: {Errors}", id, string.Join(", ", result.Errors));

            return HandleResult(result);
        }

        

        /// <summary>
        /// Deletes a service (soft delete)
        /// </summary>
        /// <param name="id">The service ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting service: {Id}", id);

            var result = await _mediator.Send(new DeleteServiceCommand { Id = id });
            return HandleControlResult(result);
        }

        /// <summary>
        /// Restores a soft-deleted service
        /// </summary>
        /// <param name="id">The service ID</param>
        /// <returns>The restored service</returns>
        [HttpPost("{id:guid}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(Guid id)
        {
            _logger.LogInformation("Restoring service: {Id}", id);

            var result = await _mediator.Send(new RestoreServiceCommand { Id = id });
            return HandleResult(result);
        }

        /// <summary>
        /// Restores multiple soft-deleted services
        /// </summary>
        /// <param name="command">The IDs of services to restore</param>
        /// <returns>Number of restored services</returns>
        [HttpPost("restore-batch")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreBatch([FromBody] RestoreServicesBatchCommand command)
        {
            _logger.LogInformation("Restoring {Count} services", command.Ids.Count);

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        #endregion

        #region Service Features

        /// <summary>
        /// Gets features for a specific service
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <returns>List of features for the service</returns>
        [HttpGet("{serviceId:guid}/features")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ServiceFeatureDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeatures(Guid serviceId)
        {
            var result = await _mediator.Send(new GetServiceFeaturesByServiceIdQuery { ServiceId = serviceId });
            return HandleResult(result);
        }

        /// <summary>
        /// Gets paginated features for a service
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="paging">Pagination parameters</param>
        /// <returns>Paginated list of features</returns>
        [HttpGet("{serviceId:guid}/features/paged")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedList<ServiceFeatureDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeaturesPaged(Guid serviceId, [FromQuery] PaginationParams paging)
        {
            var result = await _mediator.Send(new GetServiceFeaturesPagedQuery
            {
                ServiceId = serviceId,
                Paging = paging
            });
            return HandleResult(result);
        }

        /// <summary>
        /// Creates a new service feature
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="command">The feature data</param>
        /// <returns>The created feature</returns>
        [HttpPost("{serviceId:guid}/features")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceFeatureDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFeature(Guid serviceId, [FromBody] CreateServiceFeatureCommand command)
        {
            if (serviceId != command.ServiceId)
                return BadRequest(new ProblemDetails { Detail = "Service ID mismatch" });

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return HandleResult(result);

            return CreatedAtAction(nameof(GetFeatures), new { serviceId }, result.Data);
        }

        /// <summary>
        /// Updates a service feature
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="featureId">The feature ID</param>
        /// <param name="dto">The updated feature data</param>
        /// <returns>The updated feature</returns>
        [HttpPut("{serviceId:guid}/features/{featureId:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceFeatureDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFeature(Guid serviceId, Guid featureId, [FromBody] UpdateServiceFeatureDto dto)
        {
            var command = new UpdateServiceFeatureCommand { Id = featureId, UpdateServiceFeatureDto = dto };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Deletes a service feature
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="featureId">The feature ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{serviceId:guid}/features/{featureId:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFeature(Guid serviceId, Guid featureId)
        {
            var result = await _mediator.Send(new DeleteServiceFeatureCommand { Id = featureId });
            return HandleControlResult(result);
        }

        /// <summary>
        /// Restores a deleted service feature
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="featureId">The feature ID</param>
        /// <returns>The restored feature</returns>
        [HttpPost("{serviceId:guid}/features/{featureId:guid}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceFeatureDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreFeature(Guid serviceId, Guid featureId)
        {
            var result = await _mediator.Send(new RestoreServiceFeatureCommand { Id = featureId });
            return HandleResult(result);
        }

        #endregion
    }
}