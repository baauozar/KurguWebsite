using KurguWebsite.Application.Common.Interfaces.Repositories;
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
    public class ContactMessageRepository : GenericRepository<ContactMessage>, IContactMessageRepository
    {
        public ContactMessageRepository(KurguWebsiteDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<ContactMessage>> GetUnreadMessagesAsync()
        {
            return await _dbSet
                .Where(m => !m.IsRead)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ContactMessage>> GetUnrepliedMessagesAsync()
        {
            return await _dbSet
                .Where(m => !m.IsReplied)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _dbSet.CountAsync(m => !m.IsRead);
        }
    }
}