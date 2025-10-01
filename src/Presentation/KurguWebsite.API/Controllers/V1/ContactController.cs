using Asp.Versioning;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using KurguWebsite.Application.Features.ContactMessages.Commands;
using KurguWebsite.Application.Features.ContactMessages.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ContactController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ContactController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Submits a new message from the public contact form.
        /// </summary>
        /// <param name="command">The command containing the contact message data.</param>
        /// <returns>The submitted message data.</returns>
        [HttpPost("submit")]
        [AllowAnonymous]
        [EnableRateLimiting("ContactFormLimit")]
        [ProducesResponseType(typeof(ContactMessageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> SubmitContactForm(SubmitContactMessageCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (result.Data is null)
                return BadRequest("No data was returned for a successful operation.");

            return CreatedAtAction(nameof(GetMessage), new { id = result.Data.Id }, result.Data);
        }


        /// <summary>
        /// Gets a paginated list of all contact messages. (Admin/Manager Only)
        /// </summary>
        /// <param name="queryParams">Parameters for pagination.</param>
        /// <returns>A paginated list of contact messages.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(PaginatedList<ContactMessageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetContactMessages([FromQuery] QueryParameters queryParams)
        {
            var query = new GetPaginatedContactMessagesQuery { Params = queryParams };
            var result = await _mediator.Send(query);
            return Ok(result.Data);
        }

        /// <summary>
        /// Gets a specific contact message by its ID. (Admin/Manager Only)
        /// </summary>
        /// <param name="id">The GUID of the contact message.</param>
        /// <returns>The requested contact message.</returns>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ContactMessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMessage(Guid id)
        {
            var result = await _mediator.Send(new GetContactMessageByIdQuery { Id = id });
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Marks a specific message as read. (Admin/Manager Only)
        /// </summary>
        /// <param name="id">The ID of the message to mark as read.</param>
        /// <returns>A success message.</returns>
        [HttpPatch("{id:guid}/mark-read")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _mediator.Send(new MarkContactMessageAsReadCommand { Id = id });
            return result.Succeeded ? Ok(new { message = "Message marked as read" }) : NotFound(result.Errors);
        }

        /// <summary>
        /// Marks a specific message as replied. (Admin/Manager Only)
        /// </summary>
        /// <param name="id">The ID of the message to mark as replied.</param>
        /// <returns>A success message.</returns>
        [HttpPatch("{id:guid}/mark-replied")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MarkAsReplied(Guid id)
        {
            var result = await _mediator.Send(new MarkContactMessageAsRepliedCommand { Id = id });
            return result.Succeeded ? Ok(new { message = "Message marked as replied" }) : NotFound(result.Errors);
        }
    }
}