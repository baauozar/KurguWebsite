using KurguWebsite.Infrastructure.Identity;
using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace KurguWebsite.Persistence.Context
{
    public class KurguWebsiteDbContext : IdentityDbContext<
        ApplicationUser,            // TUser
        IdentityRole<Guid>,         // TRole
        Guid,                       // TKey
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
    {
        public KurguWebsiteDbContext(DbContextOptions<KurguWebsiteDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceFeature> ServiceFeatures { get; set; }
        public DbSet<CaseStudy> CaseStudies { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<CompanyInfo> CompanyInfo { get; set; }
        public DbSet<ProcessStep> ProcessSteps { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Rename Identity tables
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateAuditableEntities();
            return base.SaveChanges();
        }

        private void UpdateAuditableEntities()
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.SetCreatedBy(GetCurrentUserId());
                else if (entry.State == EntityState.Modified)
                    entry.Entity.SetModifiedBy(GetCurrentUserId());
            }
        }

        private string GetCurrentUserId()
        {
            return "System"; // Replace with ICurrentUserService in real app
        }
    }
}
