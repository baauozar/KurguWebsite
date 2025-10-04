using Asp.Versioning;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Application.Features.Testimonials.Commands;
using KurguWebsite.Application.Features.Testimonials.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    /// <summary>
    /// Manages customer testimonials
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TestimonialsController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TestimonialsController> _logger;

        public TestimonialsController(IMediator mediator, ILogger<TestimonialsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region Public Endpoints

        /// <summary>
        /// Gets all active testimonials
        /// </summary>
        /// <returns>A list of active testimonials</returns>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<TestimonialDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetActiveTestimonialsQuery());
            return HandleResult(result);
        }

        /// <summary>
        /// Gets featured testimonials
        /// </summary>
        /// <returns>A list of featured testimonials</returns>
        [HttpGet("featured")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<TestimonialDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeatured()
        {
            // Assuming you'll create this query
            var result = await _mediator.Send(new GetActiveTestimonialsQuery());
            var featured = result.Data?.Where(t => t.IsFeatured).ToList() ?? new List<TestimonialDto>();
            return Ok(featured);
        }

        /// <summary>
        /// Gets testimonials with high ratings
        /// </summary>
        /// <param name="minRating">Minimum rating (default: 4)</param>
        /// <returns>A list of high-rated testimonials</returns>
        [HttpGet("high-rated")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<TestimonialDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHighRated([FromQuery] int minRating = 4)
        {
            var result = await _mediator.Send(new GetHighRatingTestimonialsQuery { MinRating = minRating });
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a random testimonial for display
        /// </summary>
        /// <returns>A random testimonial</returns>
        [HttpGet("random")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRandom()
        {
            var result = await _mediator.Send(new GetRandomTestimonialQuery());
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a specific testimonial by ID
        /// </summary>
        /// <param name="id">The testimonial ID</param>
        /// <returns>The requested testimonial</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetTestimonialByIdQuery { Id = id });
            return HandleResult(result);
        }

        #endregion

        #region Admin Endpoints

        /// <summary>
        /// Gets a paginated list of testimonials for management
        /// </summary>
        /// <param name="queryParams">Pagination and filter parameters</param>
        /// <returns>A paginated list of testimonials</returns>
        [HttpGet("admin/paginated")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(PaginatedList<TestimonialDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated([FromQuery] QueryParameters queryParams)
        {
            var result = await _mediator.Send(new GetPaginatedTestimonialsQuery { Params = queryParams });
            return HandleResult(result);
        }

        /// <summary>
        /// Creates a new testimonial
        /// </summary>
        /// <param name="command">The testimonial data</param>
        /// <returns>The created testimonial</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTestimonialCommand command)
        {
            _logger.LogInformation("Creating new testimonial from: {ClientName}", command.ClientName);

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create testimonial: {Errors}", string.Join(", ", result.Errors));
                return HandleResult(result);
            }

            _logger.LogInformation("Testimonial created successfully with ID: {Id}", result.Data?.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result.Data);
        }

        /// <summary>
        /// Updates an existing testimonial
        /// </summary>
        /// <param name="id">The testimonial ID</param>
        /// <param name="command">The updated testimonial data</param>
        /// <returns>The updated testimonial</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTestimonialCommand command)
        {
            if (id != command.Id)
                return BadRequest(new ProblemDetails { Detail = "ID mismatch between route and body" });

            _logger.LogInformation("Updating testimonial: {Id}", id);

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                _logger.LogWarning("Failed to update testimonial {Id}: {Errors}", id, string.Join(", ", result.Errors));

            return HandleResult(result);
        }

        /// <summary>
        /// Deletes a testimonial (soft delete)
        /// </summary>
        /// <param name="id">The testimonial ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting testimonial: {Id}", id);

            var result = await _mediator.Send(new DeleteTestimonialCommand { Id = id });
            return HandleControlResult(result);
        }

        /// <summary>
        /// Restores a soft-deleted testimonial
        /// </summary>
        /// <param name="id">The testimonial ID</param>
        /// <returns>The restored testimonial</returns>
        [HttpPost("{id:guid}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(Guid id)
        {
            _logger.LogInformation("Restoring testimonial: {Id}", id);

            var result = await _mediator.Send(new RestoreTestimonialCommand { Id = id });
            return HandleResult(result);
        }

        /// <summary>
        /// Restores multiple soft-deleted testimonials
        /// </summary>
        /// <param name="command">The IDs of testimonials to restore</param>
        /// <returns>Number of restored testimonials</returns>
        [HttpPost("restore-batch")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreBatch([FromBody] RestoreTestimonialBatchCommand command)
        {
            _logger.LogInformation("Restoring {Count} testimonials", command.Ids.Count);

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Updates the featured status of a testimonial
        /// </summary>
        /// <param name="id">The testimonial ID</param>
        /// <param name="isFeatured">Whether the testimonial should be featured</param>
        /// <returns>The updated testimonial</returns>
        [HttpPatch("{id:guid}/featured")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFeaturedStatus(Guid id, [FromBody] bool isFeatured)
        {
            _logger.LogInformation("Updating featured status for testimonial {Id} to {IsFeatured}", id, isFeatured);

            // Get the existing testimonial
            var getResult = await _mediator.Send(new GetTestimonialByIdQuery { Id = id });
            if (!getResult.Succeeded)
                return HandleResult(getResult);

            // Update with the featured status
            var updateCommand = new UpdateTestimonialCommand
            {
                Id = id,
                ClientName = getResult.Data.ClientName,
                ClientTitle = getResult.Data.ClientTitle,
                CompanyName = getResult.Data.CompanyName,
                Content = getResult.Data.Content,
                Rating = getResult.Data.Rating,
                ClientImagePath = getResult.Data.ClientImagePath,
                IsFeatured = isFeatured,
                IsActive = getResult.Data.IsActive,
                DisplayOrder = getResult.Data.DisplayOrder
            };

            var result = await _mediator.Send(updateCommand);
            return HandleResult(result);
        }

        /// <summary>
        /// Updates the display order of a testimonial
        /// </summary>
        /// <param name="id">The testimonial ID</param>
        /// <param name="displayOrder">The new display order</param>
        /// <returns>The updated testimonial</returns>
        [HttpPatch("{id:guid}/display-order")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDisplayOrder(Guid id, [FromBody] int displayOrder)
        {
            _logger.LogInformation("Updating display order for testimonial {Id} to {DisplayOrder}", id, displayOrder);

            // Get the existing testimonial
            var getResult = await _mediator.Send(new GetTestimonialByIdQuery { Id = id });
            if (!getResult.Succeeded)
                return HandleResult(getResult);

            // Update with the new display order
            var updateCommand = new UpdateTestimonialCommand
            {
                Id = id,
                ClientName = getResult.Data.ClientName,
                ClientTitle = getResult.Data.ClientTitle,
                CompanyName = getResult.Data.CompanyName,
                Content = getResult.Data.Content,
                Rating = getResult.Data.Rating,
                ClientImagePath = getResult.Data.ClientImagePath,
                IsFeatured = getResult.Data.IsFeatured,
                IsActive = getResult.Data.IsActive,
                DisplayOrder = displayOrder
            };

            var result = await _mediator.Send(updateCommand);
            return HandleResult(result);
        }

        #endregion
    }
}