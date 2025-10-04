using KurguWebsite.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KurguWebsite.API.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for all API controllers
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Handles the result from application layer commands/queries
        /// </summary>
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result == null)
                return ServerError("No result returned from operation");

            if (!result.Succeeded)
            {
                // Check for specific error codes
                if (result.ErrorCode == ErrorCodes.EntityNotFound)
                    return NotFound(CreateProblemDetails(result));

                if (result.ErrorCode == ErrorCodes.ValidationError)
                    return BadRequest(CreateProblemDetails(result));

                if (result.ErrorCode == ErrorCodes.DuplicateEntity)
                    return Conflict(CreateProblemDetails(result));

                if (result.ErrorCode == ErrorCodes.Unauthorized)
                    return Unauthorized(CreateProblemDetails(result));

                if (result.ErrorCode == ErrorCodes.Forbidden)
                    return Forbidden(CreateProblemDetails(result));

                // Default to BadRequest for other failures
                return BadRequest(CreateProblemDetails(result));
            }

            // Handle partial success (for batch operations)
            if (result.IsPartialSuccess)
            {
                return Ok(new
                {
                    data = result.Data,
                    message = result.Message,
                    isPartialSuccess = true
                });
            }

            // Success with no data (for delete operations)
            if (result.Data == null)
                return NoContent();

            // Success with data
            return Ok(result.Data);
        }

        /// <summary>
        /// Handles control results (operations without return data)
        /// </summary>
        protected IActionResult HandleControlResult(ControlResult result)
        {
            if (result == null)
                return ServerError("No result returned from operation");

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
                    return NotFound(CreateProblemDetails(result));

                return BadRequest(CreateProblemDetails(result));
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a problem details response from a result
        /// </summary>
        private ProblemDetails CreateProblemDetails<T>(Result<T> result)
        {
            return new ProblemDetails
            {
                Title = "Operation Failed",
                Detail = result.Message ?? string.Join(", ", result.Errors),
                Instance = HttpContext.Request.Path,
                Extensions = { ["errors"] = result.Errors }
            };
        }

        /// <summary>
        /// Creates a problem details response from a control result
        /// </summary>
        private ProblemDetails CreateProblemDetails(ControlResult result)
        {
            return new ProblemDetails
            {
                Title = "Operation Failed",
                Detail = result.Message ?? string.Join(", ", result.Errors),
                Instance = HttpContext.Request.Path,
                Extensions = { ["errors"] = result.Errors }
            };
        }

        /// <summary>
        /// Gets the client IP address
        /// </summary>
        protected string GetIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress))
                ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            return ipAddress ?? "Unknown";
        }

        /// <summary>
        /// Gets the user agent from the request
        /// </summary>
        protected string GetUserAgent()
        {
            return HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
        }

        /// <summary>
        /// Returns a 403 Forbidden response
        /// </summary>
        protected IActionResult Forbidden(object? value = null)
        {
            if (value == null)
                value = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = "You don't have permission to access this resource"
                };

            return StatusCode(StatusCodes.Status403Forbidden, value);
        }

        /// <summary>
        /// Returns a 409 Conflict response
        /// </summary>
        protected IActionResult Conflict(object? value = null)
        {
            if (value == null)
                value = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = "The request could not be completed due to a conflict"
                };

            return StatusCode(StatusCodes.Status409Conflict, value);
        }

        /// <summary>
        /// Returns a 500 Internal Server Error response
        /// </summary>
        protected IActionResult ServerError(string message = "An unexpected error occurred")
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = message
            });
        }

        /// <summary>
        /// Gets the current user ID from claims
        /// </summary>
        protected string? GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        protected bool IsInRole(string role)
        {
            return User.IsInRole(role);
        }
    }
}