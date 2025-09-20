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
    public class CaseStudyRepository : GenericRepository<CaseStudy>, ICaseStudyRepository
    {
        public CaseStudyRepository(KurguWebsiteDbContext context) : base(context)
        {
        }

        public async Task<CaseStudy?> GetBySlugAsync(string slug)
        {
            return await _dbSet
                .Include(c => c.Service)
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IReadOnlyList<CaseStudy>> GetActiveCaseStudiesAsync()
        {
            return await _dbSet
                .Include(c => c.Service)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CompletedDate)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CaseStudy>> GetFeaturedCaseStudiesAsync()
        {
            return await _dbSet
                .Include(c => c.Service)
                .Where(c => c.IsActive && c.IsFeatured)
                .OrderByDescending(c => c.CompletedDate)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CaseStudy>> GetCaseStudiesByServiceAsync(Guid serviceId)
        {
            return await _dbSet
                .Where(c => c.IsActive && c.ServiceId == serviceId)
                .OrderByDescending(c => c.CompletedDate)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CaseStudy>> GetRecentCaseStudiesAsync(int count)
        {
            return await _dbSet
                .Include(c => c.Service)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CompletedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
        {
            var query = _dbSet.Where(c => c.Slug == slug);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}