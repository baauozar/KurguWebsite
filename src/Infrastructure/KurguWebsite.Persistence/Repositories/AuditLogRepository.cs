using KurguWebsite.Application.Interfaces.Repositories;
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
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(KurguWebsiteDbContext context) : base(context)
        {
        }
        public async Task<List<AuditLog>> GetLogsByUserIdAsync(string userId)
        {
            return await Entities
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
    }
}