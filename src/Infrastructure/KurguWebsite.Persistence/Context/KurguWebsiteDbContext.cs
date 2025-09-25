using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using KurguWebsite.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

        // --- Your DbSets remain the same ---
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
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

         
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var now = DateTime.UtcNow;
            var currentUserId = _currentUser.UserId ?? "system";

            // First, convert hard deletes into soft deletes
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Deleted && entry.Entity is AuditableEntity auditable)
                {
                    // Soft delete
                    entry.State = EntityState.Modified;
                    auditable.SoftDelete(currentUserId);
                }
            }

            // Then, apply created/modified metadata
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Id = Guid.NewGuid();
                    entry.Entity.CreatedDate = now;
                    entry.Entity.CreatedBy = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedDate = now;
                    entry.Entity.LastModifiedBy = currentUserId;
                }
            }

            // Generate audit logs for all changes
            var auditEntries = OnBeforeSaveChanges();

            // Save main changes
            var result = await base.SaveChangesAsync(cancellationToken);

            // Save audit logs
            await OnAfterSaveChanges(auditEntries, cancellationToken);

            // Dispatch domain events
            await DispatchEvents(cancellationToken);

            return result;
        }


        // --- NEW HELPER METHODS FOR AUDITING ---
        private List<AuditLog> OnBeforeSaveChanges()
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
                    IpAddress = _currentUser.IpAddress ?? "Unknown"
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
                        break;
                    case EntityState.Deleted:
                        auditEntry.Action = "DELETE";
                        auditEntry.OldValues = GetValuesJson(entry.OriginalValues);
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

        private async Task OnAfterSaveChanges(List<AuditLog> auditEntries, CancellationToken cancellationToken)
        {
            if (auditEntries == null || !auditEntries.Any()) return;

            await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }

        private string GetValuesJson(IEnumerable<PropertyEntry> properties)
        {
            var values = properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
            return JsonConvert.SerializeObject(values, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        private string GetValuesJson(PropertyValues propertyValues)
        {
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
    }
}