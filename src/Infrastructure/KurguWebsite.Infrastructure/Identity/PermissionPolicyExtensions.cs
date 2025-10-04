using KurguWebsite.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Infrastructure.Identity
{
    public static class PermissionPolicyExtensions
    {
        public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                foreach (var perm in Permissions.GetAllPermissions())
                {
                    options.AddPolicy(perm, p => p.RequireClaim("Permission", perm));
                }

                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            return services;
        }
    }
}