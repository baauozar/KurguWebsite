using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Persistence.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Infrastructure.Identity
{
    public class EnhancedAuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EnhancedAuthenticationService> _logger;
        public EnhancedAuthenticationService(
          UserManager<ApplicationUser> userManager,
          RoleManager<IdentityRole<Guid>> roleManager,
          SignInManager<ApplicationUser> signInManager,
          JwtService jwtService,
          ICurrentUserService currentUserService,
          IUnitOfWork unitOfWork,
          IHttpContextAccessor httpContextAccessor,
          ILogger<EnhancedAuthenticationService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

                if (await _userManager.IsLockedOutAsync(user))
                {
                    return Failure("Account is locked. Please try again later.");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    return Failure("Invalid credentials");
                }

                await _userManager.ResetAccessFailedCountAsync(user);

                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = await GetClaimsForUser(user, userRoles);
                var newAccessToken = _jwtService.GenerateToken(authClaims);
                var newRefreshTokenString = _jwtService.GenerateRefreshToken();

                var ipAddress = GetIpAddress();
                var refreshTokenEntity = RefreshToken.Create(user.Id, newRefreshTokenString, ipAddress);

                await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("User logged in successfully: {Email}", email);

                return Success(user.Id, user.Email!, newAccessToken, newRefreshTokenString, userRoles.ToArray());
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
                    return Failure("User with this email already exists");
                }

                var user = new ApplicationUser
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return Failure(result.Errors.Select(e => e.Description).ToArray());
                }

                await _userManager.AddToRoleAsync(user, "User");

                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = await GetClaimsForUser(user, userRoles);
                var newAccessToken = _jwtService.GenerateToken(authClaims);
                var newRefreshTokenString = _jwtService.GenerateRefreshToken();

                var ipAddress = GetIpAddress();
                var refreshTokenEntity = RefreshToken.Create(user.Id, newRefreshTokenString, ipAddress);

                await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("New user registered: {Email}", request.Email);

                return Success(user.Id, user.Email!, newAccessToken, newRefreshTokenString, userRoles.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Email}", request.Email);
                return Failure("An error occurred during registration");
            }
        }

        public async Task<AuthenticationResultModel> RefreshTokenAsync(string token, string refreshToken)
        {
            try
            {
                var principal = _jwtService.ValidateToken(token);
                if (principal == null) return Failure("Invalid token");

                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId)) return Failure("Invalid token");

                var storedToken = await _unitOfWork.RefreshTokens.GetActiveTokenAsync(refreshToken);
                if (storedToken == null || storedToken.UserId != userId)
                {
                    return Failure("Invalid refresh token");
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return Failure("User not found");

                var ipAddress = GetIpAddress();
                var newRefreshTokenString = _jwtService.GenerateRefreshToken();
                storedToken.Revoke(ipAddress, newRefreshTokenString);

                var newRefreshTokenEntity = RefreshToken.Create(userId, newRefreshTokenString, ipAddress);
                await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
                await _unitOfWork.CommitAsync();

                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = await GetClaimsForUser(user, userRoles);
                var newAccessToken = _jwtService.GenerateToken(authClaims);

                _logger.LogInformation("Token refreshed for user: {Email}", user.Email);

                return Success(user.Id, user.Email!, newAccessToken, newRefreshTokenString, userRoles.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return Failure("An error occurred during token refresh");
            }
        }

        private async Task<List<Claim>> GetClaimsForUser(ApplicationUser user, IEnumerable<string> roles)
        {
            var authClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    authClaims.AddRange(roleClaims.Where(c => c.Type == "Permission"));
                }
            }
            return authClaims;
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