// src/Infrastructure/KurguWebsite.Infrastructure/Services/PermissionService.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KurguWebsite.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public PermissionService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            // Try to parse userId as Guid
            if (!Guid.TryParse(userId, out var userGuid))
                return false;

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null) return false;

            // Check user claims
            var userClaims = await _userManager.GetClaimsAsync(user);
            if (userClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                return true;

            // Check role claims
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    if (roleClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        return true;
                }
            }

            return false;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var permissions = new HashSet<string>();

            // Try to parse userId as Guid
            if (!Guid.TryParse(userId, out var userGuid))
                return permissions.ToList();

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null) return permissions.ToList();

            // Get user permissions
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in userClaims.Where(c => c.Type == "Permission"))
            {
                permissions.Add(claim.Value);
            }

            // Get role permissions
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var claim in roleClaims.Where(c => c.Type == "Permission"))
                    {
                        permissions.Add(claim.Value);
                    }
                }
            }

            return permissions.ToList();
        }

        public async Task<bool> GrantPermissionAsync(string userId, string permission)
        {
            // Try to parse userId as Guid
            if (!Guid.TryParse(userId, out var userGuid))
                return false;

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null) return false;

            var claim = new Claim("Permission", permission);
            var result = await _userManager.AddClaimAsync(user, claim);

            return result.Succeeded;
        }

        public async Task<bool> RevokePermissionAsync(string userId, string permission)
        {
            // Try to parse userId as Guid
            if (!Guid.TryParse(userId, out var userGuid))
                return false;

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null) return false;

            var claims = await _userManager.GetClaimsAsync(user);
            var claim = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == permission);

            if (claim == null) return true;

            var result = await _userManager.RemoveClaimAsync(user, claim);
            return result.Succeeded;
        }

        public async Task<bool> GrantPermissionsToRoleAsync(string roleName, List<string> permissions)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return false;

            foreach (var permission in permissions)
            {
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
            }

            return true;
        }

        public async Task<bool> RevokePermissionsFromRoleAsync(string roleName, List<string> permissions)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return false;

            var roleClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                var claim = roleClaims.FirstOrDefault(c => c.Type == "Permission" && c.Value == permission);
                if (claim != null)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }
            }

            return true;
        }

        public async Task<List<string>> GetRolePermissionsAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return new List<string>();

            var claims = await _roleManager.GetClaimsAsync(role);
            return claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();
        }
    }
}