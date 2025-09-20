using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Infrastructure.Caching;
using KurguWebsite.Infrastructure.Identity;
using KurguWebsite.Infrastructure.Localization;
using KurguWebsite.Infrastructure.SEO;
using KurguWebsite.Infrastructure.Services;
using KurguWebsite.Infrastructure.Services.DateTimeService;
using KurguWebsite.Infrastructure.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens; // <-- This is required for AddCors

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

            // File Storage
            services.AddScoped<IFileUploadService, LocalFileStorageService>();

            // Identity & Authentication
            services.AddScoped<JwtService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
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
                    options.RequireHttpsMetadata = false;
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
                        ClockSkew = TimeSpan.Zero
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

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        var allowedOrigins = configuration["Cors:AllowedOrigins"]?.Split(',') ?? new[] { "*" };
                        builder.WithOrigins(allowedOrigins)
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    });
            });

            return services;
        }
    }
}