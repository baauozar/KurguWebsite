using KurguWebsite.Domain.Constants;
using KurguWebsite.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KurguWebsite.Infrastructure.Identity
{
    public static class PermissionPolicyExtensions
    {
        public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                foreach (var perm in GetAllPermissions())
                {
                    options.AddPolicy(perm, p => p.AddRequirements(new PermissionRequirement(perm)));
                }
            });
            return services;
        }

        private static IEnumerable<string> GetAllPermissions()
        {
            var allPermissions = new List<string>();
            var nestedTypes = typeof(Permissions).GetNestedTypes();

            foreach (var type in nestedTypes)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                allPermissions.AddRange(fields.Select(fi => (string)fi.GetValue(null)));
            }
            return allPermissions;
        }
    }
}