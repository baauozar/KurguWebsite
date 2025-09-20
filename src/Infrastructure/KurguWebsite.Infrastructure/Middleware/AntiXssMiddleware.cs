using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace KurguWebsite.Infrastructure.Middleware
{
    public class AntiXssMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Regex _scriptPattern;
        private readonly Regex _eventHandlerPattern;

        public AntiXssMiddleware(RequestDelegate next)
        {
            _next = next;
            _scriptPattern = new Regex(@"<script[^>]*>.*?</script>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            _eventHandlerPattern = new Regex(@"on\w+\s*=",
                RegexOptions.IgnoreCase);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check for XSS in query strings
            if (context.Request.QueryString.HasValue)
            {
                var queryString = context.Request.QueryString.Value;
                if (ContainsXss(queryString))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Potential XSS detected in query string");
                    return;
                }
            }

            // Check for XSS in form data
            if (context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();
                foreach (var field in form)
                {
                    if (ContainsXss(field.Value))
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync($"Potential XSS detected in form field: {field.Key}");
                        return;
                    }
                }
            }

            await _next(context);
        }

        private bool ContainsXss(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var decodedInput = HttpUtility.HtmlDecode(input);

            return _scriptPattern.IsMatch(decodedInput) ||
                   _eventHandlerPattern.IsMatch(decodedInput) ||
                   decodedInput.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
                   decodedInput.Contains("vbscript:", StringComparison.OrdinalIgnoreCase) ||
                   decodedInput.Contains("onload=", StringComparison.OrdinalIgnoreCase) ||
                   decodedInput.Contains("onerror=", StringComparison.OrdinalIgnoreCase) ||
                   decodedInput.Contains("<iframe", StringComparison.OrdinalIgnoreCase) ||
                   decodedInput.Contains("<object", StringComparison.OrdinalIgnoreCase) ||
                   decodedInput.Contains("<embed", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class AntiXssMiddlewareExtensions
    {
        public static IApplicationBuilder UseAntiXss(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AntiXssMiddleware>();
        }
    }
}
