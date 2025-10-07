// File: src/Infrastructure/KurguWebsite.Infrastructure/DependencyInjection.cs

using KurguWebsite.Application.Common.Interfaces;
// If your concrete implementations live in these namespaces, keep them.
// Otherwise, change them to the correct namespaces where your classes are.
using KurguWebsite.Infrastructure.Caching;                    // MemoryCacheService
using KurguWebsite.Infrastructure.Identity;                   // CurrentUserService, EnhancedAuthenticationService (if here)
using KurguWebsite.Infrastructure.Localization;               // LocalizationService
using KurguWebsite.Infrastructure.SEO;                        // SeoService
using KurguWebsite.Infrastructure.Services;                   // AppEnvironment, BackgroundJobService
using KurguWebsite.Infrastructure.Services.DateTimeService;   // DateTimeService
using KurguWebsite.Infrastructure.Services.Email;             // EmailService
using KurguWebsite.Persistence.Context;     // DbContext lives in Persistence
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
// using KurguWebsite.Infrastructure.FileStorage;            // EnhancedFileStorageService (if in a different ns)

namespace KurguWebsite.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // -------------------------
            // Identity (EF store)
            // -------------------------
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<KurguWebsiteDbContext>()
            .AddDefaultTokenProviders().AddRoles<IdentityRole<Guid>>();

            // -------------------------
            // JWT Authentication (optional)
            // -------------------------
            var jwt = configuration.GetSection("Jwt");
            var secret = jwt["Secret"] ?? jwt["Key"]; // support either key name

            if (!string.IsNullOrWhiteSpace(secret))
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                services.AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = true; // more secure
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt["Issuer"],

                        ValidateAudience = true,
                        ValidAudience = jwt["Audience"],

                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key
                    };

                    o.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                                context.Response.Headers["Token-Expired"] = "true";

                            return Task.CompletedTask;
                        }
                    };
                });
            }

            // -------------------------
            // Core cross-cutting services
            // -------------------------
            services.AddHttpContextAccessor();

            services.AddScoped<IAppEnvironment, AppEnvironment>();
            services.AddSingleton<IDateTime, DateTimeService>();

            // Avoid name clash with Microsoft.AspNetCore.Authentication.IAuthenticationService
            services.AddScoped<
                KurguWebsite.Application.Common.Interfaces.IAuthenticationService,
                EnhancedAuthenticationService>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Caching
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // File Storage (adjust namespace if different)
            services.AddScoped<IFileUploadService, EnhancedFileStorageService>();

            // Localization
            services.AddLocalization(o => o.ResourcesPath = "Resources");
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.Configure<LocalizationOptions>(configuration.GetSection("Localization"));

            // SEO
            services.AddScoped<ISeoService, SeoService>();

            // Email
            services.AddScoped<IEmailService, EmailService>();
            // JWT helper
            services.AddScoped<JwtService>();  // ✅ required so DI can construct EnhancedAuthenticationService
            services.AddScoped<IPermissionService, PermissionService>();

            // Register Authorization
        


            // Background Jobs
            services.AddSingleton<IBackgroundJobService, BackgroundJobService>();

            // -------------------------
            // CORS
            // -------------------------
            services.AddCors(options =>
            {
                options.AddPolicy("Production", builder =>
                {
                    var allowedOrigins = configuration
                        .GetSection("Cors:Production:AllowedOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();

                    if (allowedOrigins.Length > 0)
                    {
                        builder
                            .WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .SetPreflightMaxAge(TimeSpan.FromHours(1));
                    }
                });

                options.AddPolicy("Development", builder =>
                    builder
                        .WithOrigins("https://localhost:44319", "http://localhost:7858")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            // -------------------------
            // Rate Limiting (ASP.NET Core 8)
            // -------------------------
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name
                                      ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                      ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddPolicy("LoginLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(15)
                        }));

                options.AddPolicy("RegistrationLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 3,
                            QueueLimit = 0,
                            Window = TimeSpan.FromHours(1)
                        }));

                options.AddPolicy("ApiLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.User.Identity?.Name
                        ?? httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddPolicy("FileUploadLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.User.Identity?.Name
                        ?? httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(10)
                        }));

                options.AddPolicy("PasswordResetLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 3,
                            QueueLimit = 0,
                            Window = TimeSpan.FromHours(1)
                        }));

                options.AddPolicy("ContactFormLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            QueueLimit = 0,
                            Window = TimeSpan.FromHours(1)
                        }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.", token);
                };
            });

            return services;
        }
    }
}
