using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Context
{
    public class KurguWebsiteDbContextFactory : IDesignTimeDbContextFactory<KurguWebsiteDbContext>
    {
        public KurguWebsiteDbContext CreateDbContext(string[] args)
        {
            // Adjust path to point to your API project
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../Presentation/KurguWebsite.API"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<KurguWebsiteDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new KurguWebsiteDbContext(optionsBuilder.Options);
        }
    }
}