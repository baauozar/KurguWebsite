// src/Core/KurguWebsite.Domain/Specifications/ServiceFeatureSpecifications.cs
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Domain.Specifications
{
    public class FeaturesByServiceSpecification : BaseSpecification<ServiceFeature>
    {
        public FeaturesByServiceSpecification(Guid serviceId)
            : base(sf => sf.ServiceId == serviceId && !sf.IsDeleted)
        {
            ApplyOrderBy(sf => sf.DisplayOrder);
        }
    }

    public class ActiveFeaturesSpecification : BaseSpecification<ServiceFeature>
    {
        public ActiveFeaturesSpecification()
            : base(sf => !sf.IsDeleted)
        {
            AddInclude(sf => sf.Service);
            ApplyOrderBy(sf => sf.DisplayOrder);
        }
    }

    public class FeatureSearchSpecification : BaseSpecification<ServiceFeature>
    {
        public FeatureSearchSpecification(string? searchTerm, Guid? serviceId = null)
            : base(sf =>
                !sf.IsDeleted &&
                (serviceId == null || sf.ServiceId == serviceId) &&
                (string.IsNullOrEmpty(searchTerm) ||
                 sf.Title.Contains(searchTerm) ||
                 sf.Description.Contains(searchTerm)))
        {
            AddInclude(sf => sf.Service);
            ApplyOrderBy(sf => sf.DisplayOrder);
        }
    }
}