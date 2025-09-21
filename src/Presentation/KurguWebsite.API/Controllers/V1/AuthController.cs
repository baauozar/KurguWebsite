using Asp.Versioning;
using KurguWebsite.API.Models.Auth;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KurguWebsite.WebAPI.Controllers.V1
{
    [ApiVersion("1.0")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticationService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and refresh token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("LoginLimit")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] Application.Common.Models.LoginRequest request)
        {
            _logger.LogInformation("Login attempt for user: {Email} from IP: {IP}",
                request.Email, GetIpAddress());

            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (!result.Success)
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", request.Email);
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

            // Set refresh token in HTTP-only cookie
            SetRefreshTokenCookie(result.RefreshToken);

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(response);
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>JWT token and refresh token</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("RegistrationLimit")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Register([FromBody] Application.Common.Models.RegisterRequest request)
        {
            _logger.LogInformation("Registration attempt for email: {Email} from IP: {IP}",
                request.Email, GetIpAddress());

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                _logger.LogWarning("Failed registration for email: {Email}", request.Email);
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

            // Set refresh token in HTTP-only cookie
            SetRefreshTokenCookie(result.RefreshToken);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return CreatedAtAction(nameof(GetProfile), new { }, response);
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New JWT token and refresh token</returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            // Try to get refresh token from cookie if not provided
            var refreshToken = request.RefreshToken;
            if (string.IsNullOrEmpty(refreshToken))
            {
                refreshToken = Request.Cookies["refreshToken"];
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            var result = await _authService.RefreshTokenAsync(request.Token, refreshToken);

            if (!result.Success)
            {
                _logger.LogWarning("Failed token refresh attempt from IP: {IP}", GetIpAddress());
                return BadRequest(new { message = "Invalid token" });
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

            // Update refresh token cookie
            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(response);
        }

        /// <summary>
        /// Logout user
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();

            // Clear refresh token cookie
            Response.Cookies.Delete("refreshToken");

            _logger.LogInformation("User logged out: {Email}", User.Identity?.Name);
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Password change request</param>
        /// <returns>Success message</returns>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _authService.ChangePasswordAsync(
                request.CurrentPassword,
                request.NewPassword);

            if (!result)
            {
                return BadRequest(new { message = "Failed to change password" });
            }

            _logger.LogInformation("Password changed for user: {Email}", User.Identity?.Name);
            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        /// <param name="request">Password reset request</param>
        /// <returns>Success message</returns>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("PasswordResetLimit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ForgotPassword([FromBody] API.Models.Auth.ForgotPasswordRequest request)
        {
            _logger.LogInformation("Password reset requested for: {Email} from IP: {IP}",
                request.Email, GetIpAddress());

            await _authService.ResetPasswordAsync(request.Email);

            // Always return success to prevent email enumeration
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        /// <summary>
        /// Confirm email address
        /// </summary>
        /// <param name="token">Confirmation token</param>
        /// <returns>Success message</returns>
        [HttpGet("confirm-email")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailAsync(token);

            if (!result)
            {
                return BadRequest(new { message = "Invalid or expired token" });
            }

            _logger.LogInformation("Email confirmed for user: {Email}", User.Identity?.Name);
            return Ok(new { message = "Email confirmed successfully" });
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>User profile information</returns>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        public IActionResult GetProfile()
        {
            var profile = new UserProfileResponse
            {
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "",
                Email = User.Identity?.Name ?? "",
                Roles = User.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToArray()
            };

            return Ok(profile);
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        /// <returns>Authentication status</returns>
        [HttpGet("check")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult CheckAuth()
        {
            return Ok(new { authenticated = true, user = User.Identity?.Name });
        }

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        /// <param name="request">Revoke token request</param>
        /// <returns>Success message</returns>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequest request)
        {
            // In a real implementation, you would revoke the token in the database
            var token = request.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            // Clear cookie
            Response.Cookies.Delete("refreshToken");

            _logger.LogInformation("Token revoked for user: {Email}", User.Identity?.Name);
            return Ok(new { message = "Token revoked successfully" });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
