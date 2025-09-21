using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KurguWebsite.WebAPI.Filters
{
    public class RequireHttpsAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            if (!filterContext.HttpContext.Request.IsHttps)
            {
                HandleNonHttpsRequest(filterContext);
            }
        }

        protected void HandleNonHttpsRequest(AuthorizationFilterContext filterContext)
        {
            if (filterContext.HttpContext.Request.Method != "GET")
            {
                filterContext.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
            else
            {
                var request = filterContext.HttpContext.Request;
                var url = $"https://{request.Host}{request.Path}{request.QueryString}";
                filterContext.Result = new RedirectResult(url, permanent: true);
            }
        }
    }
}
