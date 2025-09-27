/*using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUser;

        public AuditInterceptor(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        // Runs before EF Core saves changes
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AddAuditLogs(DbContext? context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State is EntityState.Detached or EntityState.Unchanged)
                    continue;

                var auditLog = new AuditLog
                {
                    EntityType = entry.Entity.GetType().Name,
                    EntityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "N/A",
                    UserId = _currentUser.UserId ?? "System",
                    UserName = _currentUser.UserName ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = _currentUser.IpAddress ?? "Unknown",
                    Action = entry.State switch
                    {
                        EntityState.Added => AuditAction.Create,
                        EntityState.Deleted => AuditAction.Delete,
                        EntityState.Modified => AuditAction.Update,
                        _ => AuditAction.None
                    },
                    OldValues = entry.State == EntityState.Added ? "{}"
                        : JsonConvert.SerializeObject(entry.OriginalValues.ToObject(), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    NewValues = entry.State == EntityState.Deleted ? "{}"
                        : JsonConvert.SerializeObject(entry.CurrentValues.ToObject(), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                };

                context.Set<AuditLog>().Add(auditLog);
            }
        }
    }
}
*/