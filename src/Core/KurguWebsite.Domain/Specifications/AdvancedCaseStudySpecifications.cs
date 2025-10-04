// src/Core/KurguWebsite.Domain/Specifications/AdvancedCaseStudySpecifications.cs
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Domain.Specifications
{
    /// <summary>
    /// Get case studies by industry
    /// </summary>
    public class CaseStudiesByIndustrySpecification : BaseSpecification<CaseStudy>
    {
        public CaseStudiesByIndustrySpecification(string industry)
            : base(cs => cs.IsActive && !cs.IsDeleted && cs.Industry == industry)
        {
            AddInclude(cs => cs.Service);
            AddInclude(cs => cs.Metrics);
            ApplyOrderByDescending(cs => cs.CompletedDate);
        }
    }

    /// <summary>
    /// Get case studies by date range
    /// </summary>
    public class CaseStudiesByDateRangeSpecification : BaseSpecification<CaseStudy>
    {
        public CaseStudiesByDateRangeSpecification(DateTime startDate, DateTime endDate)
            : base(cs =>
                cs.IsActive &&
                !cs.IsDeleted &&
                cs.CompletedDate >= startDate &&
                cs.CompletedDate <= endDate)
        {
            AddInclude(cs => cs.Service);
            ApplyOrderByDescending(cs => cs.CompletedDate);
        }
    }

    /// <summary>
    /// Get case studies with technologies
    /// </summary>
    public class CaseStudiesWithTechnologiesSpecification : BaseSpecification<CaseStudy>
    {
        public CaseStudiesWithTechnologiesSpecification(List<string> technologies)
            : base(cs =>
                cs.IsActive &&
                !cs.IsDeleted &&
                cs.Technologies.Any(t => technologies.Contains(t)))
        {
            AddInclude(cs => cs.Service);
            AddInclude(cs => cs.Metrics);
            ApplyOrderByDescending(cs => cs.CompletedDate);
        }
    }

    /// <summary>
    /// Advanced case study search
    /// </summary>
    public class AdvancedCaseStudySearchSpecification : BaseSpecification<CaseStudy>
    {
        public AdvancedCaseStudySearchSpecification(
            string? searchTerm,
            string? industry,
            Guid? serviceId,
            bool? isFeatured,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize)
            : base(cs =>
                !cs.IsDeleted &&
                cs.IsActive &&
                (industry == null || cs.Industry == industry) &&
                (serviceId == null || cs.ServiceId == serviceId) &&
                (isFeatured == null || cs.IsFeatured == isFeatured) &&
                (fromDate == null || cs.CompletedDate >= fromDate) &&
                (toDate == null || cs.CompletedDate <= toDate) &&
                (string.IsNullOrEmpty(searchTerm) ||
                 cs.Title.Contains(searchTerm) ||
                 cs.Description.Contains(searchTerm) ||
                 cs.ClientName.Contains(searchTerm)))
        {
            AddInclude(cs => cs.Service);
            AddInclude(cs => cs.Metrics);
            ApplyOrderByDescending(cs => cs.CompletedDate);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}