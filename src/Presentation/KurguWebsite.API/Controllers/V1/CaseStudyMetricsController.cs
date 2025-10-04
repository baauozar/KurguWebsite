using Asp.Versioning;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.Features.CaseStudies.Commands;
using KurguWebsite.Application.Features.CaseStudies.Queries;
using KurguWebsite.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization; // Add this using statement
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    /// <summary>
    /// Manages metrics associated with case studies. Access is restricted to administrators.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")] // This secures the entire controller
    public class CaseStudyMetricsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public CaseStudyMetricsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Gets a specific case study metric by its unique ID (Admin only).
        /// </summary>
        /// <param name="id">The GUID of the case study metric.</param>
        /// <returns>The requested case study metric.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CaseStudyMetricDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return HandleResult(await _mediator.Send(new GetCaseStudyMetricByIdQuery { Id = id }));
        }

        /// <summary>
        /// Gets all metrics associated with a specific case study  (Admin only).
        /// </summary>
        /// <param name="caseStudyId">The GUID of the parent case study.</param>
        /// <returns>A list of metrics for the specified case study.</returns>
        [HttpGet("case-study/{caseStudyId}")]
        [ProducesResponseType(typeof(List<CaseStudyMetricDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCaseStudyId(Guid caseStudyId)
        {
            return HandleResult(await _mediator.Send(new GetCaseStudyMetricsByCaseStudyIdQuery { CaseStudyId = caseStudyId }));
        }

        /// <summary>
        /// Creates a new case study metric (Admin only).
        /// </summary>
        /// <param name="command">The data for the new metric  (Admin only).</param>
        /// <returns>A result indicating success or failure.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateCaseStudyMetricCommand command)
        {
            return HandleResult(await _mediator.Send(command));
        }

        /// <summary>
        /// Updates an existing case study metric  (Admin only).
        /// </summary>
        /// <param name="id">The GUID of the metric to update.</param>
        /// <param name="dto">The updated data for the metric.</param>
        /// <returns>A result indicating success or failure.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, UpdateCaseStudyMetricDto dto)
        {
            var command = new UpdateCaseStudyMetricCommand { Id = id, UpdateCaseStudyMetricDto = dto };
            return HandleResult(await _mediator.Send(command));
        }

        /// <summary>
        /// Deletes a case study metric  (Admin only).
        /// </summary>
        /// <param name="id">The GUID of the metric to delete.</param>
        /// <returns>A result indicating success or failure.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            ControlResult data = await _mediator.Send(new DeleteCaseStudyMetricCommand { Id = id });

            if (data is null)
                return HandleResult(Result<ControlResult>.NotFound(nameof(CaseStudyMetric), id));

            return HandleResult(Result<ControlResult>.Success(data, "Deleted successfully"));
        }
    }
}