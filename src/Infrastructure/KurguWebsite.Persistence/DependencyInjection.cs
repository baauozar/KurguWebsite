using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Infrastructure.Identity;
using KurguWebsite.Persistence.Context;
using KurguWebsite.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<KurguWebsiteDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(KurguWebsiteDbContext).Assembly.FullName)));

            // Add Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<KurguWebsiteDbContext>()
            .AddDefaultTokenProviders();

            // Register repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<ICaseStudyRepository, CaseStudyRepository>();
            services.AddScoped<ITestimonialRepository, TestimonialRepository>();
            services.AddScoped<IPartnerRepository, PartnerRepository>();
            services.AddScoped<IPageRepository, PageRepository>();
            services.AddScoped<IContactMessageRepository, ContactMessageRepository>();
            services.AddScoped<ICompanyInfoRepository, CompanyInfoRepository>();
            services.AddScoped<IProcessStepRepository, ProcessStepRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}