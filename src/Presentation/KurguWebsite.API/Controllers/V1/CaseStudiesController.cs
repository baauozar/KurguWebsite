using Asp.Versioning;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.Features.CaseStudies.Commands;
using KurguWebsite.Application.Features.CaseStudies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    /// <summary>
    /// Manages case studies for the website
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CaseStudiesController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CaseStudiesController> _logger;

        public CaseStudiesController(IMediator mediator, ILogger<CaseStudiesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region Public Endpoints

        /// <summary>
        /// Gets all active case studies
        /// </summary>
        /// <returns>A list of active case studies</returns>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllCaseStudiesQuery());
            return HandleResult(result);
        }

        /// <summary>
        /// Gets featured case studies
        /// </summary>
        /// <returns>A list of featured case studies</returns>
        [HttpGet("featured")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeatured()
        {
            var result = await _mediator.Send(new GetFeaturedCaseStudiesQuery());
            return HandleResult(result);
        }

        /// <summary>
        /// Gets recent case studies
        /// </summary>
        /// <param name="count">Number of recent case studies to retrieve (default: 5)</param>
        /// <returns>A list of recent case studies</returns>
        [HttpGet("recent")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 5)
        {
            var result = await _mediator.Send(new GetRecentCaseStudiesQuery { Count = count });
            return HandleResult(result);
        }

        /// <summary>
        /// Gets case studies by service
        /// </summary>
        /// <param name="serviceId">The service ID to filter by</param>
        /// <returns>A list of case studies for the specified service</returns>
        [HttpGet("by-service/{serviceId:guid}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByService(Guid serviceId)
        {
            var result = await _mediator.Send(new GetCaseStudiesByServiceQuery { ServiceId = serviceId });
            return HandleResult(result);
        }

        /// <summary>
        /// Searches case studies with pagination and filtering
        /// </summary>
        /// <param name="query">Search parameters</param>
        /// <returns>A paginated list of case studies</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedList<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] SearchCaseStudiesQuery query)
        {
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a specific case study by ID
        /// </summary>
        /// <param name="id">The case study ID</param>
        /// <returns>The requested case study</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCaseStudyByIdQuery { Id = id });
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a specific case study by slug
        /// </summary>
        /// <param name="slug">The case study slug</param>
        /// <returns>The requested case study</returns>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var result = await _mediator.Send(new GetCaseStudyBySlugQuery { Slug = slug });
            return HandleResult(result);
        }

        #endregion

        #region Admin Endpoints

        /// <summary>
        /// Gets paginated case studies for admin panel
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>A paginated list of case studies</returns>
        [HttpGet("admin/paginated")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(PaginatedList<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetPaginatedCaseStudiesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            return HandleResult(result);
        }

        /// <summary>
        /// Creates a new case study
        /// </summary>
        /// <param name="command">The case study data</param>
        /// <returns>The created case study</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCaseStudyCommand command)
        {
            _logger.LogInformation("Creating new case study: {Title}", command.Title);

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create case study: {Errors}", string.Join(", ", result.Errors));
                return HandleResult(result);
            }

            _logger.LogInformation("Case study created successfully with ID: {Id}", result.Data?.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result.Data);
        }

        /// <summary>
        /// Updates an existing case study
        /// </summary>
        /// <param name="id">The case study ID</param>
        /// <param name="command">The updated case study data</param>
        /// <returns>The updated case study</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCaseStudyCommand command)
        {
            if (id != command.Id)
                return BadRequest(new ProblemDetails { Detail = "ID mismatch between route and body" });

            _logger.LogInformation("Updating case study: {Id}", id);

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                _logger.LogWarning("Failed to update case study {Id}: {Errors}", id, string.Join(", ", result.Errors));

            return HandleResult(result);
        }

        /// <summary>
        /// Deletes a case study (soft delete)
        /// </summary>
        /// <param name="id">The case study ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting case study: {Id}", id);

            var result = await _mediator.Send(new DeleteCaseStudyCommand { Id = id });
            return HandleControlResult(result);
        }

        /// <summary>
        /// Restores a soft-deleted case study
        /// </summary>
        /// <param name="id">The case study ID</param>
        /// <returns>The restored case study</returns>
        [HttpPost("{id:guid}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(Guid id)
        {
            _logger.LogInformation("Restoring case study: {Id}", id);

            var result = await _mediator.Send(new RestoreCaseStudyCommand { Id = id });
            return HandleResult(result);
        }

    /*    /// <summary>
        /// Restores multiple soft-deleted case studies
        /// </summary>
        /// <param name="command">The IDs of case studies to restore</param>
        /// <returns>Number of restored case studies</returns>
        [HttpPost("restore-batch")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreBatch([FromBody] RestoreCaseStudiesBatchCommand command)
        {
            _logger.LogInformation("Restoring {Count} case studies", command.Ids.Count);

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
*/
        #endregion

        #region Case Study Metrics

        /// <summary>
        /// Gets metrics for a specific case study
        /// </summary>
        /// <param name="caseStudyId">The case study ID</param>
        /// <returns>List of metrics for the case study</returns>
        [HttpGet("{caseStudyId:guid}/metrics")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CaseStudyMetricDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMetrics(Guid caseStudyId)
        {
            var result = await _mediator.Send(new GetCaseStudyMetricsByCaseStudyIdQuery { CaseStudyId = caseStudyId });
            return HandleResult(result);
        }

        /// <summary>
        /// Creates a new case study metric
        /// </summary>
        /// <param name="caseStudyId">The case study ID</param>
        /// <param name="command">The metric data</param>
        /// <returns>The created metric</returns>
        [HttpPost("{caseStudyId:guid}/metrics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyMetricDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateMetric(Guid caseStudyId, [FromBody] CreateCaseStudyMetricCommand command)
        {
            if (caseStudyId != command.CaseStudyId)
                return BadRequest(new ProblemDetails { Detail = "Case study ID mismatch" });

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return HandleResult(result);

            return CreatedAtAction(nameof(GetMetrics), new { caseStudyId }, result.Data);
        }

        /// <summary>
        /// Updates a case study metric
        /// </summary>
        /// <param name="caseStudyId">The case study ID</param>
        /// <param name="metricId">The metric ID</param>
        /// <param name="dto">The updated metric data</param>
        /// <returns>The updated metric</returns>
        [HttpPut("{caseStudyId:guid}/metrics/{metricId:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyMetricDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMetric(Guid caseStudyId, Guid metricId, [FromBody] UpdateCaseStudyMetricDto dto)
        {
            var command = new UpdateCaseStudyMetricCommand { Id = metricId, UpdateCaseStudyMetricDto = dto };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Deletes a case study metric
        /// </summary>
        /// <param name="caseStudyId">The case study ID</param>
        /// <param name="metricId">The metric ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{caseStudyId:guid}/metrics/{metricId:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMetric(Guid caseStudyId, Guid metricId)
        {
            var result = await _mediator.Send(new DeleteCaseStudyMetricCommand { Id = metricId });
            return HandleControlResult(result);
        }

        /// <summary>
        /// Restores a deleted case study metric
        /// </summary>
        /// <param name="caseStudyId">The case study ID</param>
        /// <param name="metricId">The metric ID</param>
        /// <returns>The restored metric</returns>
        [HttpPost("{caseStudyId:guid}/metrics/{metricId:guid}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyMetricDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreMetric(Guid caseStudyId, Guid metricId)
        {
            var result = await _mediator.Send(new RestoreCaseStudyMetricCommand { Id = metricId });
            return HandleResult(result);
        }

        #endregion
    }
}