using KurguWebsite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Interfaces.Repositories
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<List<AuditLog>> GetLogsByUserIdAsync(string userId);
    }
}
