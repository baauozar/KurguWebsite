// src/Infrastructure/KurguWebsite.Persistence/Repositories/CaseStudyMetricRepository.cs
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace KurguWebsite.Persistence.Repositories;

public class CaseStudyMetricRepository : GenericRepository<CaseStudyMetric>, ICaseStudyMetricRepository
{
    public CaseStudyMetricRepository(KurguWebsiteDbContext context) : base(context)
    {
    }

    public async Task<List<CaseStudyMetric>> GetByCaseStudyIdAsync(Guid caseStudyId)
    {
        // The query now correctly looks at the CaseStudyMetrics table
        return await _context.CaseStudyMetrics
            .Where(x => x.CaseStudyId == caseStudyId)
            .ToListAsync();
    }
}