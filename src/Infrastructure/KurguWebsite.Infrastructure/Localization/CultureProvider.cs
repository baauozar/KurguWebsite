using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Infrastructure.Localization
{
    public class CultureProvider : RequestCultureProvider
    {
        public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            // Check cookie first
            var cultureCookie = httpContext.Request.Cookies["culture"];
            if (!string.IsNullOrEmpty(cultureCookie))
            {
                return new ProviderCultureResult(cultureCookie);
            }

            // Check Accept-Language header
            var acceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                var languages = acceptLanguage.Split(',')
                    .Select(l => l.Split(';')[0].Trim())
                    .ToList();

                foreach (var language in languages)
                {
                    if (IsSupportedCulture(language))
                    {
                        return new ProviderCultureResult(language);
                    }
                }
            }

            // Default to English
            return await Task.FromResult(new ProviderCultureResult("en-US"));
        }

        private bool IsSupportedCulture(string culture)
        {
            var supportedCultures = new[] { "en-US", "tr-TR" };
            return supportedCultures.Contains(culture);
        }
    }
}