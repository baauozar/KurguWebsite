using AutoMapper;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using KurguWebsite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetLogsByUserIdAsync(string userId);

        // Get paginated logs
        Task<PaginatedList<AuditLog>> GetPaginatedLogsAsync(int pageNumber, int pageSize);
        IQueryable<AuditLog> Entities { get; }
        Task<PaginatedList<AuditLogDto>> GetPagedAsync(
       int pageNumber,
       int pageSize,
       IConfigurationProvider configuration,
       CancellationToken cancellationToken = default);

    }
}
