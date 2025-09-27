using KurguWebsite.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KurguWebsite.Persistence.Context
{
    public class KurguWebsiteDbContextFactory : IDesignTimeDbContextFactory<KurguWebsiteDbContext>
    {
        public KurguWebsiteDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../Presentation/KurguWebsite.API"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<KurguWebsiteDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            var mediator = new NoOpMediator(); // Your dummy mediator
            var currentUser = new DesignTimeCurrentUserService();
            return new KurguWebsiteDbContext(
               optionsBuilder.Options,
                mediator,
                currentUser
           );
        }
    }

    // This mock now fully implements the ICurrentUserService interface for design-time tools.
    public class DesignTimeCurrentUserService : ICurrentUserService
    {
        public string? UserId => null;
        public Guid? UserGuidId => null;
        public string? UserName => null;
        public string? Email => null;
        public bool IsAuthenticated => false;
        public bool IsAdmin => false;

        public string? IpAddress => null;

        public bool IsInRole(string role) => false;
    }


}