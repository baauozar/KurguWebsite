using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Infrastructure.Caching;
using KurguWebsite.Infrastructure.Identity;
using KurguWebsite.Infrastructure.Localization;
using KurguWebsite.Infrastructure.SEO;
using KurguWebsite.Infrastructure.Services;
using KurguWebsite.Infrastructure.Services.DateTimeService;
using KurguWebsite.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

namespace KurguWebsite.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Core Services
            services.AddScoped<IAppEnvironment, AppEnvironment>();
            services.AddSingleton<IDateTime, DateTimeService>();
          

            // Caching
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // File Storage (Updated with enhanced security)
            services.AddScoped<IFileUploadService, EnhancedFileStorageService>();

            // Identity & Authentication (Updated to use Enhanced version)
            services.AddScoped<JwtService>();
            services.AddScoped<IAuthenticationService, EnhancedAuthenticationService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();


            // JWT Configuration
            var jwtSettings = configuration.GetSection("Jwt");
            var secret = jwtSettings["Secret"];

            if (!string.IsNullOrEmpty(secret))
            {
                var key = Encoding.ASCII.GetBytes(secret);

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true; // Changed to true for security
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwtSettings["Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RequireExpirationTime = true
                    };

                    // Add JWT events for better security
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            }

            // Localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.Configure<LocalizationOptions>(configuration.GetSection("Localization"));

            // SEO
            services.AddScoped<ISeoService, SeoService>();

            // Email Service
            services.AddScoped<IEmailService, EmailService>();

            // Background Jobs
            services.AddSingleton<IBackgroundJobService, BackgroundJobService>();

            // HttpContext Accessor
            services.AddHttpContextAccessor();

            // FIXED CORS Configuration - More Secure
            services.AddCors(options =>
            {
                options.AddPolicy("Production", builder =>
                {
                    var allowedOrigins = configuration.GetSection("Cors:Production:AllowedOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();

                    if (allowedOrigins.Length > 0)
                    {
                        builder
                            .WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                    }
                });

                /*   options.AddPolicy("Development", builder =>
                   {
                       var devOrigins = configuration.GetSection("Cors:Development:AllowedOrigins")
                           .Get<string[]>() ?? new[] { "http://localhost:3000", "http://localhost:3001" };

                       builder
                           .WithOrigins(devOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials()
                           .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                   });*/
                options.AddPolicy("Development", builder =>
                 builder.WithOrigins("https://localhost:44319", "http://localhost:7858") // Add your UI and API ports
                .AllowAnyHeader()
                .AllowAnyMethod());
            });

            // ADD RATE LIMITING
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;

                // Global rate limit
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext => RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // Login endpoint rate limit
                options.AddPolicy("LoginLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(15)
                        }));

                // Registration endpoint rate limit
                options.AddPolicy("RegistrationLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 3,
                            QueueLimit = 0,
                            Window = TimeSpan.FromHours(1)
                        }));

                // API endpoints rate limit
                options.AddPolicy("ApiLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // File upload rate limit
                options.AddPolicy("FileUploadLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(10)
                        }));

                // Password reset rate limit
                options.AddPolicy("PasswordResetLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 3,
                            QueueLimit = 0,
                            Window = TimeSpan.FromHours(1)
                        }));

                // Contact form rate limit
                options.AddPolicy("ContactFormLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            QueueLimit = 0,
                            Window = TimeSpan.FromHours(1)
                        }));

                // Add custom rejection response
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.", cancellationToken: token);
                };
            });

            return services;
        }
    }
}