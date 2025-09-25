using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using KurguWebsite.Infrastructure.Identity;
using MediatR;
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
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;

        public KurguWebsiteDbContext(DbContextOptions<KurguWebsiteDbContext> options, IMediator mediator, ICurrentUserService currentUser) : base(options)
        {
            _mediator = mediator;
            _currentUser = currentUser;
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
        public DbSet<CaseStudyMetric> CaseStudyMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<DomainEvent>();
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

        // In KurguWebsiteDbContext.cs
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    // Set all properties for new entities here
                    entry.Entity.Id = Guid.NewGuid();
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUser.UserId ?? "system";
                }

                if (entry.State == EntityState.Modified)
                {
                    // Set all properties for modified entities here
                    entry.Entity.LastModifiedDate = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = _currentUser.UserId ?? "system";
                }
            }

            await DispatchEvents(cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task DispatchEvents(CancellationToken cancellationToken)
        {
            var domainEventEntities = ChangeTracker.Entries<BaseEntity>()
                .Select(e => e.Entity)
                // This is the corrected line: DomainEvent -> DomainEvents
                .Where(e => e.DomainEvents.Any())
                .ToArray();

            foreach (var entity in domainEventEntities)
            {
                var events = entity.DomainEvents.ToArray();
                entity.ClearDomainEvents();
                foreach (var domainEvent in events)
                {
                    // Pass the cancellation token to the publish method
                    await _mediator.Publish(domainEvent, cancellationToken);
                }
            }
        }
    }
}