using Microsoft.AspNetCore.Authorization;

namespace KurguWebsite.WebUI.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission) : base(policy: permission)
        {
        }
    }
}