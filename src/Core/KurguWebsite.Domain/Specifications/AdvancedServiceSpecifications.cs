// src/Core/KurguWebsite.Domain/Specifications/AdvancedServiceSpecifications.cs
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;

namespace KurguWebsite.Domain.Specifications
{
    /// <summary>
    /// Get services with related case studies
    /// </summary>
    public class ServicesWithCaseStudiesSpecification : BaseSpecification<Service>
    {
        public ServicesWithCaseStudiesSpecification()
            : base(s => s.IsActive && !s.IsDeleted && s.CaseStudies.Any())
        {
            AddInclude(s => s.CaseStudies);
            ApplyOrderBy(s => s.DisplayOrder);
        }
    }

    /// <summary>
    /// Get services by multiple categories
    /// </summary>
    public class ServicesByMultipleCategoriesSpecification : BaseSpecification<Service>
    {
        public ServicesByMultipleCategoriesSpecification(List<ServiceCategory> categories)
            : base(s => s.IsActive && !s.IsDeleted && categories.Contains(s.Category))
        {
            AddInclude(s => s.Features);
            ApplyOrderBy(s => s.Category);
            ApplyOrderBy(s => s.DisplayOrder);
        }
    }

    /// <summary>
    /// Get services with minimum features count
    /// </summary>
    public class ServicesWithMinimumFeaturesSpecification : BaseSpecification<Service>
    {
        public ServicesWithMinimumFeaturesSpecification(int minFeatureCount)
            : base(s => s.IsActive && !s.IsDeleted && s.Features.Count >= minFeatureCount)
        {
            AddInclude(s => s.Features);
            ApplyOrderByDescending(s => s.Features.Count);
        }
    }

    /// <summary>
    /// Advanced service search with filters
    /// </summary>
    public class AdvancedServiceSearchSpecification : BaseSpecification<Service>
    {
        public AdvancedServiceSearchSpecification(
            string? searchTerm,
            ServiceCategory? category,
            bool? isFeatured,
            bool? hasFeatures,
            int pageNumber,
            int pageSize)
            : base(s =>
                !s.IsDeleted &&
                s.IsActive &&
                (category == null || s.Category == category) &&
                (isFeatured == null || s.IsFeatured == isFeatured) &&
                (hasFeatures == null || (hasFeatures.Value ? s.Features.Any() : !s.Features.Any())) &&
                (string.IsNullOrEmpty(searchTerm) ||
                 s.Title.Contains(searchTerm) ||
                 s.Description.Contains(searchTerm) ||
                 s.ShortDescription.Contains(searchTerm)))
        {
            AddInclude(s => s.Features);
            ApplyOrderBy(s => s.DisplayOrder);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}