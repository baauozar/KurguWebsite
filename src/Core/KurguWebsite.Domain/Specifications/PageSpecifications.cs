// src/Core/KurguWebsite.Domain/Specifications/PageSpecifications.cs
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;

namespace KurguWebsite.Domain.Specifications
{
    public class ActivePagesSpecification : BaseSpecification<Page>
    {
        public ActivePagesSpecification()
            : base(p => p.IsActive && !p.IsDeleted)
        {
            ApplyOrderBy(p => p.Title);
        }
    }

    public class PageBySlugSpecification : BaseSpecification<Page>
    {
        public PageBySlugSpecification(string slug)
            : base(p => p.Slug == slug && p.IsActive && !p.IsDeleted)
        {
        }
    }

    public class PageByTypeSpecification : BaseSpecification<Page>
    {
        public PageByTypeSpecification(PageType pageType)
            : base(p => p.PageType == pageType && p.IsActive && !p.IsDeleted)
        {
        }
    }

    public class PageSearchSpecification : BaseSpecification<Page>
    {
        public PageSearchSpecification(
            string? searchTerm,
            PageType? pageType,
            bool includeInactive = false)
            : base(p =>
                !p.IsDeleted &&
                (includeInactive || p.IsActive) &&
                (pageType == null || p.PageType == pageType) &&
                (string.IsNullOrEmpty(searchTerm) ||
                 p.Title.Contains(searchTerm) ||
                 (p.Content != null && p.Content.Contains(searchTerm))))
        {
            ApplyOrderBy(p => p.Title);
        }
    }
}