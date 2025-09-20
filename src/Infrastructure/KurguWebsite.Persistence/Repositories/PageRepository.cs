using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Repositories
{
    public class PageRepository : GenericRepository<Page>, IPageRepository
    {
        public PageRepository(KurguWebsiteDbContext context) : base(context)
        {
        }

        public async Task<Page?> GetBySlugAsync(string slug)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        }

        public async Task<Page?> GetByPageTypeAsync(PageType pageType)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.PageType == pageType);
        }

        public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
        {
            var query = _dbSet.Where(p => p.Slug == slug);

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}