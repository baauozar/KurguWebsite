using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace KurguWebsite.WebUI.Helpers
{
    [HtmlTargetElement("div", Attributes = "permission")]
    [HtmlTargetElement("a", Attributes = "permission")]
    [HtmlTargetElement("button", Attributes = "permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("permission")]
        public string Permission { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !HasPermission(user, Permission))
            {
                output.SuppressOutput();
            }
        }

        private bool HasPermission(ClaimsPrincipal user, string permission)
        {
            return user.HasClaim(c => c.Type == "Permission" && c.Value == permission);
        }
    }
}