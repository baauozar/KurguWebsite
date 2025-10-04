// src/Core/KurguWebsite.Domain/Specifications/PartnerSpecifications.cs
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;

namespace KurguWebsite.Domain.Specifications
{
    public class ActivePartnersSpecification : BaseSpecification<Partner>
    {
        public ActivePartnersSpecification()
            : base(p => p.IsActive && !p.IsDeleted)
        {
            ApplyOrderBy(p => p.DisplayOrder);
        }
    }

    public class PartnersByTypeSpecification : BaseSpecification<Partner>
    {
        public PartnersByTypeSpecification(PartnerType type)
            : base(p => p.Type == type && p.IsActive && !p.IsDeleted)
        {
            ApplyOrderBy(p => p.DisplayOrder);
        }
    }

    public class PartnerSearchSpecification : BaseSpecification<Partner>
    {
        public PartnerSearchSpecification(string? searchTerm, int pageNumber, int pageSize)
            : base(p =>
                !p.IsDeleted &&
                (string.IsNullOrEmpty(searchTerm) ||
                 p.Name.Contains(searchTerm) ||
                 (p.Description != null && p.Description.Contains(searchTerm))))
        {
            ApplyOrderBy(p => p.DisplayOrder);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    public class TopPartnersSpecification : BaseSpecification<Partner>
    {
        public TopPartnersSpecification(int count)
            : base(p => p.IsActive && !p.IsDeleted)
        {
            ApplyOrderBy(p => p.DisplayOrder);
            ApplyPaging(0, count);
        }
    }
}