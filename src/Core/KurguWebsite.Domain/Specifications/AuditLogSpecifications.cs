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

    /// <summary>
    /// Search spec with filters, optional free-text search, sorting, and paging.
    /// </summary>
    public class AuditLogSearchSpecification : BaseSpecification<AuditLog>
    {
        public AuditLogSearchSpecification(
            string? userId,
            string? entityType,
            string? action,
            DateTime? fromDate,
            DateTime? toDate,
            string? searchTerm,
            string? sortColumn,
            string? sortOrder,
            int pageNumber,
            int pageSize)
            : base(al =>
                !al.IsDeleted &&
                (string.IsNullOrEmpty(userId) || al.UserId == userId) &&
                (string.IsNullOrEmpty(entityType) || al.EntityType == entityType) &&
                (string.IsNullOrEmpty(action) || (al.Action ?? "").Contains(action)) &&
                (!fromDate.HasValue || al.Timestamp >= fromDate.Value) &&
                (!toDate.HasValue || al.Timestamp <= toDate.Value) &&
                (string.IsNullOrWhiteSpace(searchTerm) ||
                    ((al.UserName ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())) ||
                    ((al.EntityType ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())) ||
                    ((al.EntityId ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())) ||
                    ((al.Action ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())) ||
                    ((al.IpAddress ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())))
            )
        {
            // Sorting
            var col = (sortColumn ?? "").ToLowerInvariant();
            var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            if (desc)
            {
                switch (col)
                {
                    case "username": ApplyOrderByDescending(al => al.UserName!); break;
                    case "entitytype": ApplyOrderByDescending(al => al.EntityType!); break;
                    case "action": ApplyOrderByDescending(al => al.Action!); break;
                    case "entityid": ApplyOrderByDescending(al => al.EntityId!); break;
                    default: ApplyOrderByDescending(al => al.Timestamp); break;
                }
            }
            else
            {
                switch (col)
                {
                    case "username": ApplyOrderBy(al => al.UserName!); break;
                    case "entitytype": ApplyOrderBy(al => al.EntityType!); break;
                    case "action": ApplyOrderBy(al => al.Action!); break;
                    case "entityid": ApplyOrderBy(al => al.EntityId!); break;
                    default: ApplyOrderBy(al => al.Timestamp); break;
                }
            }

            // Paging
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    /// <summary>
    /// Same filters as search, but NO paging/sorting — used to compute TotalCount.
    /// </summary>
    public class AuditLogSearchCountSpecification : BaseSpecification<AuditLog>
    {
        public AuditLogSearchCountSpecification(
            string? userId,
            string? entityType,
            string? action,
            DateTime? fromDate,
            DateTime? toDate,
            string? searchTerm)
            : base(al =>
                !al.IsDeleted &&
                (string.IsNullOrEmpty(userId) || al.UserId == userId) &&
                (string.IsNullOrEmpty(entityType) || al.EntityType == entityType) &&
                (string.IsNullOrEmpty(action) || (al.Action ?? "").Contains(action)) &&
                (!fromDate.HasValue || al.Timestamp >= fromDate.Value) &&
                (!toDate.HasValue || al.Timestamp <= toDate.Value) &&
                (string.IsNullOrWhiteSpace(searchTerm) ||
                    ((al.UserName ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())) ||
                    ((al.EntityType ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())) ||
                    ((al.EntityId ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())) ||
                    ((al.Action ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())) ||
                    ((al.IpAddress ?? "").ToLower().Contains(searchTerm!.Trim().ToLower())))
            )
        { }
    }
}
