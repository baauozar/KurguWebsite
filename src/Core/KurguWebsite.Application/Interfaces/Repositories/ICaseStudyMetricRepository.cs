using KurguWebsite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Interfaces.Repositories
{
    public interface ICaseStudyMetricRepository : IGenericRepository<CaseStudyMetric>
    {
        Task<List<CaseStudyMetric>> GetByCaseStudyIdAsync(Guid caseStudyId);
    }
}
