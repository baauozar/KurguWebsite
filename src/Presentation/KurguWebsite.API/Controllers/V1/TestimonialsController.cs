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
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TestimonialsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public TestimonialsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets all active testimonials.
        /// </summary>
        /// <returns>A list of active testimonials.</returns>
        [HttpGet("active")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(List<TestimonialDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            var result = await _mediator.Send(new GetActiveTestimonialsQuery());
            return Ok(result.Data);
        }

        /// <summary>
        /// Gets a single random active testimonial.
        /// </summary>
        /// <returns>A random testimonial.</returns>
        [HttpGet("random")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRandom()
        {
            var result = await _mediator.Send(new GetRandomTestimonialQuery());
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Gets a paginated list of all testimonials for management. (Admin Only)
        /// </summary>
        /// <param name="queryParams">Parameters for pagination.</param>
        /// <returns>A paginated list of testimonials.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PaginatedList<TestimonialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPaginated([FromQuery] QueryParameters queryParams)
        {
            var query = new GetPaginatedTestimonialsQuery { Params = queryParams };
            var result = await _mediator.Send(query);
            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a new testimonial. (Admin Only)
        /// </summary>
        /// <param name="command">The command containing the testimonial data.</param>
        /// <returns>The newly created testimonial.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(CreateTestimonialCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? CreatedAtAction(nameof(GetRandom), result.Data) : BadRequest(result.Errors);
        }

        // ... You would apply the same professional standards to Update and Delete endpoints ...
    }
}