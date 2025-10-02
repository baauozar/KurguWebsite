using AutoMapper;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Persistence.Context;
using KurguWebsite.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace KurguWebsite.Persistence.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly KurguWebsiteDbContext _context;

        public AuditLogRepository(KurguWebsiteDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetLogsByUserIdAsync(string userId)
        {
            return await _context.AuditLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
        public async Task<PaginatedList<AuditLog>> GetPaginatedLogsAsync(int pageNumber, int pageSize)
        {
            // Build the query and use your extension method to create the paginated list
            return await _context.AuditLogs
                .OrderByDescending(log => log.Timestamp)
                .ToPaginatedListAsync(pageNumber, pageSize);
        }
        public IQueryable<AuditLog> Entities => _context.AuditLogs;
        public Task<PaginatedList<AuditLogDto>> GetPagedAsync(
          int pageNumber,
          int pageSize,
          IConfigurationProvider configuration,
          CancellationToken cancellationToken = default)
        {
            return _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(x => x.Timestamp)
                .ToPaginatedListAsync<AuditLog, AuditLogDto>(
                    configuration,
                    pageNumber,
                    pageSize,
                    cancellationToken);
        }

        public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}