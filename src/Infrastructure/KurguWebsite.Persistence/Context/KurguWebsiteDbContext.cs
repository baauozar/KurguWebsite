using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq.Expressions;

namespace KurguWebsite.Persistence.Context
{
    public class KurguWebsiteDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;

        public KurguWebsiteDbContext(DbContextOptions<KurguWebsiteDbContext> options, IMediator mediator, ICurrentUserService currentUser) : base(options)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceFeature> ServiceFeatures { get; set; }
        public DbSet<CaseStudy> CaseStudies { get; set; }
        public DbSet<CaseStudyMetric> CaseStudyMetrics { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<CompanyInfo> CompanyInfo { get; set; }
        public DbSet<ProcessStep> ProcessSteps { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<DomainEvent>();

            // This line automatically finds and applies all IEntityTypeConfiguration classes
            // from the current assembly, including your AuditLogConfiguration.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Renaming Identity tables for clarity
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Only apply to types that inherit from AuditableEntity
                if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");

                    // (AuditableEntity)e
                    var cast = Expression.Convert(parameter, typeof(AuditableEntity));

                    // ((AuditableEntity)e).IsDeleted == false
                    var isDeletedProp = Expression.Property(cast, nameof(AuditableEntity.IsDeleted));
                    var condition = Expression.Equal(isDeletedProp, Expression.Constant(false));

                    var lambda = Expression.Lambda(condition, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            InterceptHardDeletes();
            var auditEntries = GetAuditEntries();

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Id = Guid.NewGuid();
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUser.UserId ?? "system";
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedDate = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = _currentUser.UserId ?? "system";
                }
            }

            if (auditEntries.Any())
            {
                await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
            }

            var result = await base.SaveChangesAsync(cancellationToken);
            await DispatchEvents(cancellationToken);
            return result;
        }

        private List<AuditLog> GetAuditEntries()
        {
            ChangeTracker.DetectChanges();
            var entries = new List<AuditLog>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditLog
                {
                    EntityType = entry.Entity.GetType().Name,
                    UserId = _currentUser.UserId ?? "System",
                    UserName = _currentUser.UserName ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = _currentUser.IpAddress ?? "Unknown",
                    // --- START: THE FIX ---
                    // Initialize with a default empty JSON object to prevent null database inserts.
                    OldValues = "{}",
                    NewValues = "{}"
                    // --- END: THE FIX ---
                };

                var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                if (primaryKey != null)
                {
                    auditEntry.EntityId = primaryKey.CurrentValue?.ToString() ?? "N/A";
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.Action = "CREATE";
                        auditEntry.NewValues = GetValuesJson(entry.Properties);
                        // OldValues correctly remains the default "{}"
                        break;
                    case EntityState.Deleted:
                        auditEntry.Action = "DELETE";
                        auditEntry.OldValues = GetValuesJson(entry.OriginalValues);
                        // NewValues correctly remains the default "{}"
                        break;
                    case EntityState.Modified:
                        auditEntry.Action = "UPDATE";
                        auditEntry.OldValues = GetValuesJson(entry.OriginalValues);
                        auditEntry.NewValues = GetValuesJson(entry.Properties.Where(p => p.IsModified));
                        break;
                }
                entries.Add(auditEntry);
            }
            return entries;
        }

        private string GetValuesJson(IEnumerable<PropertyEntry> properties)
        {
            if (properties == null || !properties.Any()) return "{}";
            var values = properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
            return JsonConvert.SerializeObject(values, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        private string GetValuesJson(PropertyValues propertyValues)
        {
            if (propertyValues == null) return "{}";
            var values = propertyValues.Properties.ToDictionary(p => p.Name, p => propertyValues[p]);
            return JsonConvert.SerializeObject(values, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        private async Task DispatchEvents(CancellationToken cancellationToken)
        {
            var domainEventEntities = ChangeTracker.Entries<BaseEntity>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToArray();

            foreach (var entity in domainEventEntities)
            {
                var events = entity.DomainEvents.ToArray();
                entity.ClearDomainEvents();
                foreach (var domainEvent in events)
                {
                    await _mediator.Publish(domainEvent, cancellationToken);
                }
            }
        }
        private void InterceptHardDeletes()
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.SoftDelete(_currentUser.UserId ?? "system");
                }
            }
        }
    }
}

