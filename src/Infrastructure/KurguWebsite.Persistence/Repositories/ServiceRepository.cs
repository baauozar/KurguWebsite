using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.Specifications;
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
        new private readonly KurguWebsiteDbContext _context;
        public ServiceRepository(KurguWebsiteDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Service?> GetBySlugAsync(string slug)
        {
            var spec = new ServiceBySlugSpecification(slug);
            return await GetBySpecAsync(spec);
        }

        public async Task<Service?> GetServiceWithFeaturesAsync(Guid id)
        {
            var spec = new ServiceByIdWithFeaturesSpecification(id);
            return await GetBySpecAsync(spec);
        }

        public async Task<IReadOnlyList<Service>> GetActiveServicesAsync()
        {
            var spec = new ActiveServicesSpecification();
            return await ListAsync(spec);
        }

        public async Task<IReadOnlyList<Service>> GetFeaturedServicesAsync()
        {
            var spec = new ActiveFeaturedServicesSpecification();
            return await ListAsync(spec);
        }


        public async Task<IReadOnlyList<Service>> GetServicesByCategoryAsync(ServiceCategory category)
        {
            var spec = new ServicesByCategorySpecification(category);
            return await ListAsync(spec);
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
        public IQueryable<Service> GetActiveQueryable()
        {
            return _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder);
        }
        public IQueryable<Service> GetActiveServicesQueryable()
        {
            return _dbSet.Where(s => s.IsActive);
        }
    }
}