using Asp.Versioning;
using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.WebAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [EnableRateLimiting("ApiLimit")]
    public class CaseStudiesController : BaseApiController
    {
        private readonly ICaseStudyService _caseStudyService;
        private readonly ILogger<CaseStudiesController> _logger;

        public CaseStudiesController(
            ICaseStudyService caseStudyService,
            ILogger<CaseStudiesController> logger)
        {
            _caseStudyService = caseStudyService;
            _logger = logger;
        }

        /// <summary>
        /// Get all case studies
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(IEnumerable<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCaseStudies()
        {
            var result = await _caseStudyService.GetAllAsync();

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get featured case studies
        /// </summary>
        [HttpGet("featured")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(IEnumerable<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeaturedCaseStudies()
        {
            var result = await _caseStudyService.GetFeaturedAsync();

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get case study by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCaseStudy(Guid id)
        {
            var result = await _caseStudyService.GetByIdAsync(id);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get case study by slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCaseStudyBySlug(string slug)
        {
            var result = await _caseStudyService.GetBySlugAsync(slug);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get case studies by service
        /// </summary>
        [HttpGet("service/{serviceId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<CaseStudyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCaseStudiesByService(Guid serviceId)
        {
            var result = await _caseStudyService.GetByServiceAsync(serviceId);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get paginated case studies
        /// </summary>
        [HttpGet("paginated")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginatedCaseStudies(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _caseStudyService.GetPaginatedAsync(pageNumber, pageSize);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new
            {
                result.Data?.Items,
                result.Data?.PageNumber,
                result.Data?.TotalPages,
                result.Data?.TotalCount,
                result.Data?.HasPreviousPage,
                result.Data?.HasNextPage
            });
        }

        /// <summary>
        /// Create new case study (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCaseStudy([FromBody] CreateCaseStudyDto dto)
        {
            _logger.LogInformation("Creating new case study: {Title}", dto.Title);

            var result = await _caseStudyService.CreateAsync(dto);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create case study: {Errors}", string.Join(", ", result.Errors));
                return BadRequest(new { errors = result.Errors });
            }

            _logger.LogInformation("Case study created successfully with ID: {Id}", result.Data?.Id);
            return CreatedAtAction(nameof(GetCaseStudy), new { id = result.Data?.Id }, result.Data);
        }

        /// <summary>
        /// Update case study (Admin only)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CaseStudyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCaseStudy(Guid id, [FromBody] UpdateCaseStudyDto dto)
        {
            _logger.LogInformation("Updating case study: {Id}", id);

            var result = await _caseStudyService.UpdateAsync(id, dto);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
                    return NotFound(new { message = result.Message });

                return BadRequest(new { errors = result.Errors });
            }

            _logger.LogInformation("Case study updated successfully: {Id}", id);
            return Ok(result.Data);
        }

        /// <summary>
        /// Delete case study (Admin only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCaseStudy(Guid id)
        {
            _logger.LogInformation("Deleting case study: {Id}", id);

            var result = await _caseStudyService.DeleteAsync(id);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            _logger.LogInformation("Case study deleted successfully: {Id}", id);
            return NoContent();
        }

        /// <summary>
        /// Toggle case study featured status (Admin only)
        /// </summary>
        [HttpPatch("{id:guid}/toggle-featured")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleCaseStudyFeatured(Guid id)
        {
            var result = await _caseStudyService.ToggleFeaturedAsync(id);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}