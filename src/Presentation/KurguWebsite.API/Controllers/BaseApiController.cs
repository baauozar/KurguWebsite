using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KurguWebsite.API.Controllers
{
    [ApiController]
  
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult HandleResult<T>(T result) where T : class
        {
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        protected string GetIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress))
                ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            return ipAddress ?? "Unknown";
        }

        protected string GetUserAgent()
        {
            return HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
        }

        protected  IActionResult Unauthorized(string message = "Unauthorized")
        {
            return base.Unauthorized(new { message });
        }

        protected IActionResult Forbidden(string message = "Access forbidden")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message });
        }
    }
}