using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Infrastructure.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;
        private readonly SecurityHeadersOptions _options;

        public SecurityHeadersMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<SecurityHeadersMiddleware> logger,
            SecurityHeadersOptions? options = null)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
            _options = options ?? new SecurityHeadersOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers before processing the request
            AddSecurityHeaders(context);

            // Remove server header
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("X-AspNet-Version");
            context.Response.Headers.Remove("X-AspNetMvc-Version");

            await _next(context);
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;

            // Prevent XSS attacks
            if (_options.UseXssProtection)
            {
                headers["X-XSS-Protection"] = "1; mode=block";
            }

            // Prevent MIME type sniffing
            if (_options.UseContentTypeOptions)
            {
                headers["X-Content-Type-Options"] = "nosniff";
            }

            // Prevent clickjacking
            if (_options.UseFrameOptions)
            {
                headers["X-Frame-Options"] = _options.FrameOptionsPolicy;
            }

            // Referrer policy
            if (_options.UseReferrerPolicy)
            {
                headers["Referrer-Policy"] = _options.ReferrerPolicy;
            }

            // Permissions Policy (formerly Feature Policy)
            if (_options.UsePermissionsPolicy && !string.IsNullOrEmpty(_options.PermissionsPolicy))
            {
                headers["Permissions-Policy"] = _options.PermissionsPolicy;
            }

            // Content Security Policy
            if (_options.UseContentSecurityPolicy && !string.IsNullOrEmpty(_options.ContentSecurityPolicy))
            {
                if (_options.UseContentSecurityPolicyReportOnly)
                {
                    headers["Content-Security-Policy-Report-Only"] = _options.ContentSecurityPolicy;
                }
                else
                {
                    headers["Content-Security-Policy"] = _options.ContentSecurityPolicy;
                }
            }

            // Strict Transport Security (HSTS)
            if (_options.UseHsts)
            {
                headers["Strict-Transport-Security"] = $"max-age={_options.HstsMaxAge}; includeSubDomains; preload";
            }

            // Custom headers
            foreach (var customHeader in _options.CustomHeaders)
            {
                headers[customHeader.Key] = customHeader.Value;
            }
        }
    }

    public class SecurityHeadersOptions
    {
        public bool UseXssProtection { get; set; } = true;
        public bool UseContentTypeOptions { get; set; } = true;
        public bool UseFrameOptions { get; set; } = true;
        public string FrameOptionsPolicy { get; set; } = "DENY"; // DENY, SAMEORIGIN, or ALLOW-FROM uri
        public bool UseReferrerPolicy { get; set; } = true;
        public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";
        public bool UsePermissionsPolicy { get; set; } = true;
        public string PermissionsPolicy { get; set; } = "camera=(), microphone=(), geolocation=()";
        public bool UseContentSecurityPolicy { get; set; } = true;
        public bool UseContentSecurityPolicyReportOnly { get; set; } = false;
        public string ContentSecurityPolicy { get; set; } = BuildDefaultCsp();
        public bool UseHsts { get; set; } = true;
        public int HstsMaxAge { get; set; } = 31536000; // 1 year in seconds
        public Dictionary<string, string> CustomHeaders { get; set; } = new();

        private static string BuildDefaultCsp()
        {
            return @"
                default-src 'self';
                script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net;
                style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com;
                font-src 'self' https://fonts.gstatic.com;
                img-src 'self' data: https:;
                connect-src 'self' https://api.kurguwebsite.com;
                frame-src 'self';
                object-src 'none';
                base-uri 'self';
                form-action 'self';
                frame-ancestors 'none';
                upgrade-insecure-requests;
            ".Replace("\r\n", " ").Replace("\n", " ").Trim();
        }
    }

    // Extension method to add the middleware
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(
            this IApplicationBuilder builder,
            SecurityHeadersOptions? options = null)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>(options ?? new SecurityHeadersOptions());
        }
    }
}
