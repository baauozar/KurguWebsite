using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace KurguWebsite.Infrastructure.Authorization
{
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null)
            {
                return policy;
            }

            if (policyName.StartsWith("Permissions."))
            {
                var builder = new AuthorizationPolicyBuilder();
                builder.AddRequirements(new PermissionRequirement(policyName));
                return builder.Build();
            }

            return null;
        }
    }
}