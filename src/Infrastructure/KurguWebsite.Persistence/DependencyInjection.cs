using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Persistence.Context;
using KurguWebsite.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KurguWebsite.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<KurguWebsiteDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(KurguWebsiteDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<ICaseStudyRepository, CaseStudyRepository>();
            services.AddScoped<ITestimonialRepository, TestimonialRepository>();
            services.AddScoped<IPartnerRepository, PartnerRepository>();
            services.AddScoped<IPageRepository, PageRepository>();
            services.AddScoped<IContactMessageRepository, ContactMessageRepository>();
            services.AddScoped<ICompanyInfoRepository, CompanyInfoRepository>();
            services.AddScoped<IProcessStepRepository, ProcessStepRepository>();
            services.AddScoped<IServiceUniquenessChecker, ServiceUniquenessChecker>();
            services.AddScoped<ICaseStudyUniquenessChecker, CaseStudyUniquenessChecker>();
            services.AddScoped<IPageUniquenessChecker, PageUniquenessChecker>();
            services.AddScoped<IServiceUniquenessChecker, ServiceUniquenessChecker>();


            // UoW
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
