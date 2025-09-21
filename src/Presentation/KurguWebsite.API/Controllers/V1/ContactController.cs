using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.DTOs.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
namespace KurguWebsite.WebAPI.Controllers.V1
{
    [ApiVersion("1.0")]
    public class ContactController : BaseApiController
    {
        private readonly IContactMessageService _contactService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
            IContactMessageService contactService,
            ILogger<ContactController> logger)
        {
            _contactService = contactService;
            _logger = logger;
        }

        /// <summary>
        /// Submit contact form
        /// </summary>
        [HttpPost("submit")]
        [AllowAnonymous]
        [EnableRateLimiting("ContactFormLimit")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(ContactMessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> SubmitContactForm([FromBody] CreateContactMessageDto dto)
        {
            _logger.LogInformation("Contact form submission from: {Email}, IP: {IP}",
                dto.Email, GetIpAddress());

            var result = await _contactService.SubmitMessageAsync(dto);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Contact form submission failed: {Errors}",
                    string.Join(", ", result.Errors));
                return BadRequest(new { errors = result.Errors });
            }

            _logger.LogInformation("Contact form submitted successfully from: {Email}", dto.Email);
            return Ok(new
            {
                message = "Your message has been received. We'll get back to you soon!",
                data = result.Data
            });
        }

        /// <summary>
        /// Get all contact messages (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        [EnableRateLimiting("ApiLimit")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContactMessages(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _contactService.GetPaginatedAsync(pageNumber, pageSize);

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
        /// Get unread messages (Admin only)
        /// </summary>
        [HttpGet("unread")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(IEnumerable<ContactMessageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadMessages()
        {
            var result = await _contactService.GetUnreadAsync();

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get unread message count
        /// </summary>
        [HttpGet("unread-count")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result = await _contactService.GetUnreadCountAsync();

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new { count = result.Data });
        }

        /// <summary>
        /// Get contact message by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ContactMessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMessage(Guid id)
        {
            var result = await _contactService.GetByIdAsync(id);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>
        /// Mark message as read
        /// </summary>
        [HttpPatch("{id:guid}/mark-read")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _contactService.MarkAsReadAsync(id);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            _logger.LogInformation("Message marked as read: {Id} by {User}",
                id, User.Identity?.Name);
            return Ok(new { message = "Message marked as read" });
        }

        /// <summary>
        /// Mark message as replied
        /// </summary>
        [HttpPatch("{id:guid}/mark-replied")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsReplied(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _contactService.MarkAsRepliedAsync(id, userId);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            _logger.LogInformation("Message marked as replied: {Id} by {User}",
                id, User.Identity?.Name);
            return Ok(new { message = "Message marked as replied" });
        }
    }
}
