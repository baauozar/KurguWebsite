// src/Infrastructure/KurguWebsite.Persistence/Repositories/ServiceFeatureRepository.cs
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace KurguWebsite.Persistence.Repositories;

public class ServiceFeatureRepository : GenericRepository<ServiceFeature>, IServiceFeatureRepository
{
    public ServiceFeatureRepository(KurguWebsiteDbContext context) : base(context)
    {
    }

    public async Task<List<ServiceFeature>> GetByServiceIdAsync(Guid serviceId)
    {
        return await _context.ServiceFeatures.Where(x => x.ServiceId == serviceId).ToListAsync();
    }
}