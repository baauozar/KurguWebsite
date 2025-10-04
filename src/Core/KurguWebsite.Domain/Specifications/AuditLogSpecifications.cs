// src/Core/KurguWebsite.Domain/Specifications/AuditLogSpecifications.cs
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Domain.Specifications
{
    public class AuditLogsByUserSpecification : BaseSpecification<AuditLog>
    {
        public AuditLogsByUserSpecification(string userId)
            : base(al => al.UserId == userId && !al.IsDeleted)
        {
            ApplyOrderByDescending(al => al.Timestamp);
        }
    }

    public class AuditLogsByEntitySpecification : BaseSpecification<AuditLog>
    {
        public AuditLogsByEntitySpecification(string entityType, string entityId)
            : base(al =>
                al.EntityType == entityType &&
                al.EntityId == entityId &&
                !al.IsDeleted)
        {
            ApplyOrderByDescending(al => al.Timestamp);
        }
    }

    public class AuditLogsByDateRangeSpecification : BaseSpecification<AuditLog>
    {
        public AuditLogsByDateRangeSpecification(DateTime startDate, DateTime endDate)
            : base(al =>
                !al.IsDeleted &&
                al.Timestamp >= startDate &&
                al.Timestamp <= endDate)
        {
            ApplyOrderByDescending(al => al.Timestamp);
        }
    }

    public class AuditLogSearchSpecification : BaseSpecification<AuditLog>
    {
        public AuditLogSearchSpecification(
            string? userId,
            string? entityType,
            string? action,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize)
            : base(al =>
                !al.IsDeleted &&
                (string.IsNullOrEmpty(userId) || al.UserId == userId) &&
                (string.IsNullOrEmpty(entityType) || al.EntityType == entityType) &&
                (string.IsNullOrEmpty(action) || al.Action.Contains(action)) &&
                (!fromDate.HasValue || al.Timestamp >= fromDate.Value) &&
                (!toDate.HasValue || al.Timestamp <= toDate.Value))
        {
            ApplyOrderByDescending(al => al.Timestamp);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}