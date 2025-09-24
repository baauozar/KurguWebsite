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
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CaseStudiesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public CaseStudiesController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Searches, sorts, and paginates case studies.
        /// </summary>
        /// <param name="queryParams">Parameters for searching, sorting, and pagination.</param>
        /// <returns>A paginated list of case studies.</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedList<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] QueryParameters queryParams)
        {
            var result = await _mediator.Send(new SearchCaseStudiesQuery { Params = queryParams });
            return Ok(result.Data);
        }

        /// <summary>
        /// Gets a specific case study by its unique ID.
        /// </summary>
        /// <param name="id">The GUID of the case study.</param>
        /// <returns>The requested case study.</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCaseStudyByIdQuery { Id = id });
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Gets a specific case study by its SEO-friendly slug.
        /// </summary>
        /// <param name="slug">The slug of the case study.</param>
        /// <returns>The requested case study.</returns>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var result = await _mediator.Send(new GetCaseStudyBySlugQuery { Slug = slug });
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Creates a new case study. (Admin Only)
        /// </summary>
        /// <param name="command">The command containing the case study data.</param>
        /// <returns>The newly created case study.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(CreateCaseStudyCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data) : BadRequest(result.Errors);
        }

        /// <summary>
        /// Updates an existing case study. (Admin Only)
        /// </summary>
        /// <param name="id">The ID of the case study to update.</param>
        /// <param name="command">The command containing the updated data.</param>
        /// <returns>The updated case study.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, UpdateCaseStudyCommand command)
        {
            if (id != command.Id) return BadRequest("ID mismatch");
            var result = await _mediator.Send(command);
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Deletes a case study. (Admin Only)
        /// </summary>
        /// <param name="id">The ID of the case study to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteCaseStudyCommand { Id = id });
            return result.Succeeded ? NoContent() : NotFound(result.Errors);
        }
    }
}