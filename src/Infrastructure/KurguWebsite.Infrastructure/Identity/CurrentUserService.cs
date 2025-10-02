using KurguWebsite.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace KurguWebsite.Infrastructure.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId
        {
            get
            {
                // Try to get from HTTP context
                var userId = _httpContextAccessor.HttpContext?.User?
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                // If no HTTP context (like during seeding), return system user
                return userId ?? "system";
            }
        }
        public Guid? UserGuidId
        {
            get
            {
                if (Guid.TryParse(UserId, out var guid))
                    return guid;
                return null;
            }
        }

        public string? UserName
        {
            get
            {
                var userName = _httpContextAccessor.HttpContext?.User?
                    .FindFirstValue(ClaimTypes.Name);

                return userName ?? "System";
            }
        }
        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated
                       ?? false;
            }
        }
        public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;

        public string? IpAddress
        {
            get
            {
                return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                       ?? "127.0.0.1";
            }
        }

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }
    }
}