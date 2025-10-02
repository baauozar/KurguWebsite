// src/Core/KurguWebsite.Domain/Specifications/CaseStudySpecifications.cs
using KurguWebsite.Domain.Entities;
using System;

namespace KurguWebsite.Domain.Specifications
{
    /// <summary>
    /// Get featured case studies
    /// </summary>
    public class FeaturedCaseStudiesSpecification : BaseSpecification<CaseStudy>
    {
        public FeaturedCaseStudiesSpecification()
            : base(cs => cs.IsFeatured && cs.IsActive && !cs.IsDeleted)
        {
            AddInclude(cs => cs.Service);
            AddInclude(cs => cs.Metrics);
            ApplyOrderBy(cs => cs.DisplayOrder);
        }
    }

    /// <summary>
    /// Get case studies by service
    /// </summary>
    public class CaseStudiesByServiceSpecification : BaseSpecification<CaseStudy>
    {
        public CaseStudiesByServiceSpecification(Guid serviceId)
            : base(cs => cs.ServiceId == serviceId && cs.IsActive && !cs.IsDeleted)
        {
            AddInclude(cs => cs.Metrics);
            ApplyOrderByDescending(cs => cs.CompletedDate);
        }
    }

    /// <summary>
    /// Get case study by slug with all details
    /// </summary>
    public class CaseStudyBySlugSpecification : BaseSpecification<CaseStudy>
    {
        public CaseStudyBySlugSpecification(string slug)
            : base(cs => cs.Slug == slug && !cs.IsDeleted)
        {
            AddInclude(cs => cs.Service);
            AddInclude(cs => cs.Metrics);
        }
    }

    /// <summary>
    /// Get recent case studies
    /// </summary>
    public class RecentCaseStudiesSpecification : BaseSpecification<CaseStudy>
    {
        public RecentCaseStudiesSpecification(int count)
            : base(cs => cs.IsActive && !cs.IsDeleted)
        {
            ApplyOrderByDescending(cs => cs.CompletedDate);
            ApplyPaging(0, count);
        }
    }

    /// <summary>
    /// Search case studies
    /// </summary>
    public class CaseStudySearchSpecification : BaseSpecification<CaseStudy>
    {
        public CaseStudySearchSpecification(
            string? searchTerm,
            string? industry,
            int pageNumber,
            int pageSize)
            : base(cs =>
                !cs.IsDeleted &&
                (string.IsNullOrEmpty(searchTerm) ||
                 cs.Title.Contains(searchTerm) ||
                 cs.Description.Contains(searchTerm) ||
                 cs.ClientName.Contains(searchTerm)) &&
                (string.IsNullOrEmpty(industry) || cs.Industry == industry))
        {
            AddInclude(cs => cs.Service);
            ApplyOrderByDescending(cs => cs.CompletedDate);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}