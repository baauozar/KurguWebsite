using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KurguWebsite.Infrastructure.Identity
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<AuthenticationResultModel> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Failure("User does not exist");

            if (!await _userManager.CheckPasswordAsync(user, password))
                return Failure("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user.Id, user.Email, roles.ToArray());
            var refreshToken = _jwtService.GenerateRefreshToken();

            return Success(user.Id, user.Email, token, refreshToken, roles.ToArray());
        }

        public async Task<AuthenticationResultModel> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Failure("User with this email already exists");

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
                return Failure(result.Errors.Select(e => e.Description).ToArray());

            await _userManager.AddToRoleAsync(user, "User");
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user.Id, user.Email, roles.ToArray());
            var refreshToken = _jwtService.GenerateRefreshToken();

            return Success(user.Id, user.Email, token, refreshToken, roles.ToArray());
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            await _signInManager.SignOutAsync();
            return true;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<AuthenticationResultModel> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null) return Failure("Invalid token");

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return Failure("Invalid user ID in token");

            var user = await _userManager.FindByIdAsync(userId.ToString()); // Identity requires string here
            if (user == null) return Failure("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            var newToken = _jwtService.GenerateToken(user.Id, user.Email, roles.ToArray());
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            return Success(user.Id, user.Email, newToken, newRefreshToken, roles.ToArray());
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // TODO: send email via IEmailService
            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }
        public async Task<bool> LogoutAsync(Guid userId)
        {
            // We don't need userId for SignOut
            await _signInManager.SignOutAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString()); // Convert Guid → string
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString()); // Convert Guid → string
            if (user == null) return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        // ----------- Helper methods -----------

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
