// Areas/Admin/Controllers/BaseAdminController.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Domain.Constants;
using KurguWebsite.WebUI.Areas.Admin.AdminControllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using static KurguWebsite.Domain.Constants.Permissions;

namespace KurguWebsite.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public abstract class BaseAdminController : BaseController
    {
        protected readonly IPermissionService PermissionService;

        protected BaseAdminController(
            IMediator mediator,
            ILogger logger,
            IPermissionService permissionService)
            : base(mediator, logger)
        {
            PermissionService = permissionService;
        }

        #region Admin-Specific Helpers

        protected string GetCurrentUserId()
        {
            return User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "System";
        }

        protected string GetCurrentUserName()
        {
            return User?.Identity?.Name ?? "Unknown";
        }

        protected bool IsUserAdmin()
        {
            return User?.IsInRole("Admin") ?? false;
        }

        #endregion

        #region Authorization Helpers

        /// <summary>
        /// Check if current user has a specific permission
        /// </summary>
        protected async Task<bool> HasPermissionAsync(string permission)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || userId == "System")
                return false;

            return await PermissionService.HasPermissionAsync(userId, permission);
        }

        /// <summary>
        /// Check if current user has a specific permission (synchronous version)
        /// </summary>
        protected bool CheckPermission(string permission)
        {
            // Check claims directly for synchronous operation
            return User?.HasClaim(c => c.Type == "Permission" && c.Value == permission) ?? false;
        }

        /// <summary>
        /// Check if user has any of the specified permissions
        /// </summary>
        protected async Task<bool> HasAnyPermissionAsync(params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (await HasPermissionAsync(permission))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if user has all of the specified permissions
        /// </summary>
        protected async Task<bool> HasAllPermissionsAsync(params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (!await HasPermissionAsync(permission))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns Forbidden result if user doesn't have permission
        /// </summary>
        protected async Task<IActionResult?> CheckPermissionOrForbidAsync(string permission)
        {
            if (!await HasPermissionAsync(permission))
            {
                SetErrorMessage("You don't have permission to perform this action");
                return Forbid();
            }
            return null;
        }

        /// <summary>
        /// Get all permissions for current user
        /// </summary>
        protected async Task<List<string>> GetCurrentUserPermissionsAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || userId == "System")
                return new List<string>();

            return await PermissionService.GetUserPermissionsAsync(userId);
        }

        #endregion

        #region Breadcrumb Helpers

        protected List<BreadcrumbItem> GetBreadcrumbs(params (string text, string? url)[] items)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Text = "Dashboard", Url = Url.Action("Index", "Dashboard", new { area = "Admin" }), IsActive = false }
            };

            for (int i = 0; i < items.Length; i++)
            {
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Text = items[i].text,
                    Url = items[i].url,
                    IsActive = i == items.Length - 1
                });
            }

            ViewData["Breadcrumbs"] = breadcrumbs;
            return breadcrumbs;
        }

        #endregion
    }
}