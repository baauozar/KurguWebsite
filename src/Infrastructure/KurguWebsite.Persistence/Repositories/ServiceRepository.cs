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
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        public ServiceRepository(KurguWebsiteDbContext context) : base(context)
        {
        }

        public async Task<Service?> GetBySlugAsync(string slug)
        {
            return await _dbSet
                .Include(s => s.Features)
                .FirstOrDefaultAsync(s => s.Slug == slug);
        }

        public async Task<Service?> GetServiceWithFeaturesAsync(Guid id)
        {
            return await _dbSet
                .Include(s => s.Features.OrderBy(f => f.DisplayOrder))
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IReadOnlyList<Service>> GetActiveServicesAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Service>> GetFeaturedServicesAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive && s.IsFeatured)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Service>> GetServicesByCategoryAsync(ServiceCategory category)
        {
            return await _dbSet
                .Where(s => s.IsActive && s.Category == category)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
        {
            var query = _dbSet.Where(s => s.Slug == slug);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<Service?> GetBySlugWithFeaturesAsync(string slug)
        {
            return await _dbSet
                .Include(s => s.Features) // This line loads the related features
                .FirstOrDefaultAsync(s => s.Slug == slug && s.IsActive);
        }
    }
}