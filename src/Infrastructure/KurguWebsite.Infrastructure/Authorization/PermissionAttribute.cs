using Microsoft.AspNetCore.Authorization;

namespace KurguWebsite.Infrastructure.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission) : base(permission) { }
    }
}