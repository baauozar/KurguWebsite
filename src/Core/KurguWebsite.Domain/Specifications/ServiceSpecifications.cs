// src/Core/KurguWebsite.Domain/Specifications/ServiceSpecifications.cs
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;


namespace KurguWebsite.Domain.Specifications
{
    /// <summary>
    /// Get all active and featured services
    /// </summary>
    public class ActiveFeaturedServicesSpecification : BaseSpecification<Service>
    {
        public ActiveFeaturedServicesSpecification()
            : base(s => s.IsActive && s.IsFeatured && !s.IsDeleted)
        {
            AddInclude(s => s.Features);
            ApplyOrderBy(s => s.DisplayOrder);
        }
    }

    /// <summary>
    /// Get active services by category
    /// </summary>
    public class ServicesByCategorySpecification : BaseSpecification<Service>
    {
        public ServicesByCategorySpecification(ServiceCategory category)
            : base(s => s.Category == category && s.IsActive && !s.IsDeleted)
        {
            ApplyOrderBy(s => s.DisplayOrder);
        }
    }

    /// <summary>
    /// Get service by slug with all related data
    /// </summary>
    public class ServiceBySlugSpecification : BaseSpecification<Service>
    {
        public ServiceBySlugSpecification(string slug)
            : base(s => s.Slug == slug && !s.IsDeleted)
        {
            AddInclude(s => s.Features);
            AddInclude(s => s.CaseStudies);
        }
    }

    /// <summary>
    /// Search services with pagination
    /// </summary>
    public class ServiceSearchSpecification : BaseSpecification<Service>
    {
        public ServiceSearchSpecification(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            bool includeInactive = false)
            : base(s =>
                !s.IsDeleted &&
                (includeInactive || s.IsActive) &&
                (string.IsNullOrEmpty(searchTerm) ||
                 s.Title.Contains(searchTerm) ||
                 s.Description.Contains(searchTerm)))
        {
            ApplyOrderBy(s => s.Title);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    /// <summary>
    /// Get service by ID with features
    /// </summary>
    public class ServiceByIdWithFeaturesSpecification : BaseSpecification<Service>
    {
        public ServiceByIdWithFeaturesSpecification(Guid id)
            : base(s => s.Id == id && !s.IsDeleted)
        {
            AddInclude(s => s.Features);
        }
    }

    /// <summary>
    /// Get all active services ordered by display order
    /// </summary>
    public class ActiveServicesSpecification : BaseSpecification<Service>
    {
        public ActiveServicesSpecification()
            : base(s => s.IsActive && !s.IsDeleted)
        {
            ApplyOrderBy(s => s.DisplayOrder);
        }
    }
}