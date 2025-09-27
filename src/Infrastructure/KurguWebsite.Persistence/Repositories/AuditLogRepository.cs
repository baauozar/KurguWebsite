using KurguWebsite.Application.Common.Extensions;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Application.Mappings;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .PaginatedListAsync(pageNumber, pageSize);
        }
        public IQueryable<AuditLog> Entities => _context.AuditLogs;
    }
}