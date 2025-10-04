// src/Core/KurguWebsite.Domain/Specifications/CaseStudyMetricSpecifications.cs
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Domain.Specifications
{
    public class MetricsByCaseStudySpecification : BaseSpecification<CaseStudyMetric>
    {
        public MetricsByCaseStudySpecification(Guid caseStudyId)
            : base(m => m.CaseStudyId == caseStudyId && !m.IsDeleted)
        {
            ApplyOrderBy(m => m.DisplayOrder);
        }
    }

    public class ActiveMetricsSpecification : BaseSpecification<CaseStudyMetric>
    {
        public ActiveMetricsSpecification()
            : base(m => !m.IsDeleted)
        {
            AddInclude(m => m.CaseStudy);
            ApplyOrderBy(m => m.DisplayOrder);
        }
    }
}