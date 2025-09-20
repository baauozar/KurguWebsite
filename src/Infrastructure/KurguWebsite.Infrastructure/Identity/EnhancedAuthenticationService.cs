using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Infrastructure.Identity
{
    public class EnhancedAuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EnhancedAuthenticationService> _logger;

        public EnhancedAuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            ILogger<EnhancedAuthenticationService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private string GetIpAddress()
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
                ipAddress = "127.0.0.1";

            // Handle IPv6 localhost
            if (ipAddress == "::1")
                ipAddress = "127.0.0.1";

            return ipAddress;
        }

        public async Task<AuthenticationResultModel> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt for non-existent user: {Email}", email);
                    return Failure("Invalid credentials");
                }

                // Check if account is locked
                if (await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("Login attempt for locked account: {Email}", email);
                    return Failure("Account is locked. Please try again later.");
                }

                // Check password
                var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("Account locked after failed attempt: {Email}", email);
                        return Failure("Account is locked due to multiple failed attempts. Please try again later.");
                    }

                    _logger.LogWarning("Invalid password for user: {Email}", email);
                    return Failure("Invalid credentials");
                }

                // Reset access failed count on successful login
                await _userManager.ResetAccessFailedCountAsync(user);

                // Generate tokens
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user.Id, user.Email!, roles.ToArray());
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Store refresh token in database
                var ipAddress = GetIpAddress();
                var refreshTokenEntity = RefreshToken.Create(user.Id, refreshToken, ipAddress);

                await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("User logged in successfully: {Email}", email);

                return Success(user.Id, user.Email!, token, refreshToken, roles.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", email);
                return Failure("An error occurred during login");
            }
        }

        public async Task<AuthenticationResultModel> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                    return Failure("User with this email already exists");
                }

                var user = new ApplicationUser
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("User registration failed for {Email}: {Errors}",
                        request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return Failure(result.Errors.Select(e => e.Description).ToArray());
                }

                await _userManager.AddToRoleAsync(user, "User");
                var roles = await _userManager.GetRolesAsync(user);

                // Generate tokens
                var token = _jwtService.GenerateToken(user.Id, user.Email!, roles.ToArray());
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Store refresh token
                var ipAddress = GetIpAddress();
                var refreshTokenEntity = RefreshToken.Create(user.Id, refreshToken, ipAddress);

                await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("New user registered: {Email}", request.Email);

                return Success(user.Id, user.Email!, token, refreshToken, roles.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Email}", request.Email);
                return Failure("An error occurred during registration");
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                // Revoke all refresh tokens for current user
                if (_currentUserService.UserGuidId.HasValue)
                {
                    var ipAddress = GetIpAddress();
                    await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                        _currentUserService.UserGuidId.Value, ipAddress);
                }

                await _signInManager.SignOutAsync();

                _logger.LogInformation("User logged out: {UserId}", _currentUserService.UserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", _currentUserService.UserId);
                return false;
            }
        }

        public async Task<AuthenticationResultModel> RefreshTokenAsync(string token, string refreshToken)
        {
            try
            {
                var principal = _jwtService.ValidateToken(token);
                if (principal == null)
                {
                    _logger.LogWarning("Invalid token during refresh");
                    return Failure("Invalid token");
                }

                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid user ID in token during refresh");
                    return Failure("Invalid token");
                }

                // Get refresh token from database
                var storedToken = await _unitOfWork.RefreshTokens.GetActiveTokenAsync(refreshToken);
                if (storedToken == null || storedToken.UserId != userId)
                {
                    _logger.LogWarning("Invalid refresh token for user: {UserId}", userId);
                    return Failure("Invalid refresh token");
                }

                // Revoke old token and create new one
                var ipAddress = GetIpAddress();
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                storedToken.Revoke(ipAddress, newRefreshToken);

                // Create new refresh token
                var newRefreshTokenEntity = RefreshToken.Create(userId, newRefreshToken, ipAddress);
                await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
                await _unitOfWork.CommitAsync();

                // Get user and generate new JWT
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User not found during token refresh: {UserId}", userId);
                    return Failure("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var newToken = _jwtService.GenerateToken(user.Id, user.Email!, roles.ToArray());

                _logger.LogInformation("Token refreshed for user: {Email}", user.Email);

                return Success(user.Id, user.Email!, newToken, newRefreshToken, roles.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return Failure("An error occurred during token refresh");
            }
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                if (_currentUserService.UserId == null) return false;

                var user = await _userManager.FindByIdAsync(_currentUserService.UserId);
                if (user == null) return false;

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    // Revoke all refresh tokens when password changes
                    var ipAddress = GetIpAddress();
                    await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, ipAddress);

                    _logger.LogInformation("Password changed for user: {Email}", user.Email);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", _currentUserService.UserId);
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal if user exists
                    return true;
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // TODO: Send email with reset token via IEmailService
                _logger.LogInformation("Password reset token generated for user: {Email}", email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password reset token for: {Email}", email);
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            try
            {
                if (_currentUserService.UserId == null) return false;

                var user = await _userManager.FindByIdAsync(_currentUserService.UserId);
                if (user == null) return false;

                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                    _logger.LogInformation("Email confirmed for user: {Email}", user.Email);

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user: {UserId}", _currentUserService.UserId);
                return false;
            }
        }

        private AuthenticationResultModel Success(Guid userId, string email, string token, string refreshToken, string[] roles)
        {
            return new AuthenticationResultModel
            {
                Success = true,
                UserId = userId,
                Email = email,
                Token = token,
                RefreshToken = refreshToken,
                Roles = roles
            };
        }

        private AuthenticationResultModel Failure(params string[] errors)
        {
            return new AuthenticationResultModel
            {
                Success = false,
                Errors = errors
            };
        }
    }
}