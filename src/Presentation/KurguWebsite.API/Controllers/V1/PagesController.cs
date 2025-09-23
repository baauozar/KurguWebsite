using Asp.Versioning;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.Features.Pages.Commands;
using KurguWebsite.Application.Features.Pages.Queries;
using KurguWebsite.Domain.Enums;
using KurguWebsite.WebAPI.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PagesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public PagesController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets page data for the Home page.
        /// </summary>
        /// <returns>The aggregated data required for the home page.</returns>
        [HttpGet("home")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(HomePageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHomePageData()
        {
            var result = await _mediator.Send(new GetHomePageDataQuery());
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Gets a page by its specific type (e.g., About, Contact).
        /// </summary>
        /// <param name="pageType">The type of the page.</param>
        /// <returns>The content of the specified page.</returns>
        [HttpGet("type/{pageType}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(PageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByType(PageType pageType)
        {
            var result = await _mediator.Send(new GetPageByTypeQuery { PageType = pageType });
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Updates the content of a specific page. (Admin Only)
        /// </summary>
        /// <param name="id">The ID of the page to update.</param>
        /// <param name="command">The command containing the updated page content.</param>
        /// <returns>The updated page data.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdatePage(Guid id, UpdatePageContentCommand command)
        {
            if (id != command.Id) return BadRequest("ID mismatch");
            var result = await _mediator.Send(command);
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }
    }
}