using Asp.Versioning;
using KurguWebsite.API.Models.Auth;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace KurguWebsite.WebAPI.Controllers.V1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="request">The user's login credentials.</param>
        /// <returns>An authentication response with tokens and user info.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("LoginLimit")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for user: {Email}", request.Email);
            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (!result.Success)
            {
                return BadRequest(new { message = string.Join(", ", result.Errors) });
            }

            var response = new AuthResponse
            {
                UserId = result.UserId!.Value,
                Email = result.Email,
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                Roles = result.Roles,
                ExpiresAt = DateTime.UtcNow.AddHours(1) // Or based on your token settings
            };
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(response);
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">The registration details for the new user.</param>
        /// <returns>An authentication response with tokens for the new user.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("RegistrationLimit")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { message = string.Join(", ", result.Errors) });
            }

            var response = new AuthResponse
            {
                UserId = result.UserId!.Value,
                Email = result.Email,
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                Roles = result.Roles,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            SetRefreshTokenCookie(result.RefreshToken);
            return CreatedAtAction(nameof(GetProfile), new { }, response);
        }

        /// <summary>
        /// Changes the password for the currently authenticated user.
        /// </summary>
        /// <param name="request">The current and new password details.</param>
        /// <returns>A success message.</returns>
        [HttpPost("change-password")]
      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _authService.ChangePasswordAsync(request.CurrentPassword, request.NewPassword);

            if (!result)
            {
                return BadRequest(new { message = "Failed to change password. Please check your current password." });
            }

            _logger.LogInformation("Password changed successfully for user: {Email}", User.Identity?.Name);
            return Ok(new { message = "Password changed successfully." });
        }

        /// <summary>
        /// Gets the profile information for the currently authenticated user.
        /// </summary>
        /// <returns>The user's profile.</returns>
        [HttpGet("profile")]
    
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetProfile()
        {
            var profile = new UserProfileResponse
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
                Roles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToArray()
            };
            return Ok(profile);
        }

        // This is a helper method and does not need to be a public API endpoint.
        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Should be true in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}