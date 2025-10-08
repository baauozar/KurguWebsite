using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Infrastructure.Caching;
using KurguWebsite.Infrastructure.Identity;
using KurguWebsite.Infrastructure.Localization;
using KurguWebsite.Infrastructure.SEO;
using KurguWebsite.Infrastructure.Services;
using KurguWebsite.Infrastructure.Services.DateTimeService;
using KurguWebsite.Infrastructure.Services.Email;
using KurguWebsite.Persistence.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

namespace KurguWebsite.Infrastructure
{
    public static class DependencyInjection
    {
        // --------------------------------------------------------------------
        //  CORE (UI + API için ortak kayıtlar)
        // --------------------------------------------------------------------
        private static IServiceCollection AddInfrastructureCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            // Identity (DbContext = KurguWebsiteDbContext, ApplicationUser = aynı tip!)
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // UI için daha yumuşatmak istersen burada yapabilirsin
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<KurguWebsiteDbContext>()
            .AddDefaultTokenProviders()
            .AddRoles<IdentityRole<Guid>>();

            // Authorization Policies (Permissions.* varsa burada ekleyebilirsin)
            // services.AddAuthorization(options =>
            // {
            //     foreach (var perm in KurguWebsite.Domain.Constants.Permissions.GetAllPermissions())
            //         options.AddPolicy(perm, p => p.RequireClaim("Permission", perm));
            // });

            // Cross-cutting services
            services.AddSingleton<IDateTime, DateTimeService>();
            services.AddScoped<IAppEnvironment, AppEnvironment>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddScoped<IFileUploadService, EnhancedFileStorageService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISeoService, SeoService>();
            services.AddScoped<JwtService>();               // EnhancedAuthenticationService için
            services.AddScoped<IPermissionService, PermissionService>();

            services.AddScoped<
                KurguWebsite.Application.Common.Interfaces.IAuthenticationService,
                EnhancedAuthenticationService>();

            // Localization
            services.AddLocalization(o => o.ResourcesPath = "Resources");
            services.Configure<LocalizationOptions>(configuration.GetSection("Localization"));
            services.AddSingleton<ILocalizationService, LocalizationService>();

            // Background jobs
            services.AddSingleton<IBackgroundJobService, BackgroundJobService>();

            return services;
        }

        // --------------------------------------------------------------------
        //  API (JWT, CORS, RateLimiter)
        // --------------------------------------------------------------------
        public static IServiceCollection AddInfrastructureApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructureCore(configuration);

            var jwt = configuration.GetSection("Jwt");
            var secret = jwt["Secret"] ?? jwt["Key"];

            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Jwt:Secret (veya Jwt:Key) ayarlanmalı (API için).");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = true;
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
                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception is SecurityTokenExpiredException)
                            ctx.Response.Headers["Token-Expired"] = "true";
                        return Task.CompletedTask;
                    }
                };
            });

            // CORS (API)
            services.AddCors(options =>
            {
                options.AddPolicy("Production", builder =>
                {
                    var allowedOrigins = configuration
                        .GetSection("Cors:Production:AllowedOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();

                    if (allowedOrigins.Length > 0)
                    {
                        builder.WithOrigins(allowedOrigins)
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials()
                               .SetIsOriginAllowedToAllowWildcardSubdomains()
                               .SetPreflightMaxAge(TimeSpan.FromHours(1));
                    }
                });

                options.AddPolicy("Development", builder =>
                    builder.WithOrigins("https://localhost:44319", "http://localhost:7858")
                           .AllowAnyHeader()
                           .AllowAnyMethod());
            });

            // Rate Limiting (API)
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.User.Identity?.Name
                        ?? httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous",
                        _ => new FixedWindowRateLimiterOptions
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

                // ... diğer özel politikalar ...
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
                };
            });

            return services;
        }

        // --------------------------------------------------------------------
        //  WEB UI (Cookie default; JWT YOK)
        // --------------------------------------------------------------------
        public static IServiceCollection AddInfrastructureWeb(this IServiceCollection services, IConfiguration configuration,
            Action<CookieAuthenticationOptions>? cookieConfigure = null)
        {
            services.AddInfrastructureCore(configuration);

            // Identity Application cookie’yi default yap
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            });

            services.ConfigureApplicationCookie(o =>
            {
                o.LoginPath = "/Admin/Auth/Login";
                o.AccessDeniedPath = "/Admin/Auth/AccessDenied";
                o.Cookie.HttpOnly = true;
                o.ExpireTimeSpan = TimeSpan.FromHours(2);

                // Dışarıdan override istersen:
                cookieConfigure?.Invoke(o);
            });

     

            return services;
        }
    }
}
