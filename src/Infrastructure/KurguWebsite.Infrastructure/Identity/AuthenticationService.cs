using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; // Required for JwtRegisteredClaimNames

namespace KurguWebsite.Infrastructure.Identity
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly ICurrentUserService _currentUserService;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _currentUserService = currentUserService;
        }

        public async Task<AuthenticationResultModel> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Failure("User does not exist");

            if (!await _userManager.CheckPasswordAsync(user, password))
                return Failure("Invalid credentials");

            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = await GetClaimsForUser(user, userRoles);
            var token = _jwtService.GenerateToken(authClaims);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return Success(user.Id, user.Email, token, refreshToken, userRoles.ToArray());
        }

        public async Task<AuthenticationResultModel> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null) return Failure("User with this email already exists");

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

            // Correctly generate claims and token for the new user
            var authClaims = await GetClaimsForUser(user, roles);
            var token = _jwtService.GenerateToken(authClaims);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return Success(user.Id, user.Email, token, refreshToken, roles.ToArray());
        }

        public async Task<AuthenticationResultModel> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null) return Failure("Invalid token");

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return Failure("Invalid user ID in token");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Failure("User not found");

            // Regenerate token with fresh claims
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = await GetClaimsForUser(user, userRoles);
            var newToken = _jwtService.GenerateToken(authClaims);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            return Success(user.Id, user.Email, newToken, newRefreshToken, userRoles.ToArray());
        }

        private async Task<List<Claim>> GetClaimsForUser(ApplicationUser user, IEnumerable<string> roles)
        {
            var authClaims = new List<Claim>
            {
                // --- FIX IS HERE ---
                new(ClaimTypes.NameIdentifier, user.Id.ToString()), // Convert Guid to string
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

        // ... Other methods (LogoutAsync, etc.) remain unchanged ...

        #region Unchanged Methods
        public async Task<bool> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return true;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (_currentUserService.UserId == null) return false;
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId);
            if (user == null) return false;
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // TODO: Send email
            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            if (_currentUserService.UserId == null) return false;
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId);
            if (user == null) return false;
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        private AuthenticationResultModel Success(Guid userId, string email, string token, string refreshToken, string[] roles)
        {
            return new AuthenticationResultModel { Success = true, UserId = userId, Email = email, Token = token, RefreshToken = refreshToken, Roles = roles };
        }

        private AuthenticationResultModel Failure(params string[] errors)
        {
            return new AuthenticationResultModel { Success = false, Errors = errors };
        }
        #endregion
    }
}