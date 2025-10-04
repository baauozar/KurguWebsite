using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.Services;

namespace KurguWebsite.Domain.Entities
{
    public class Page : AuditableEntity, ISeoEntity, IActivatable
    {
        public string Title { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public string? Content { get; private set; }
        public PageType PageType { get; private set; }
        public bool IsActive { get; private set; }

        // Hero Section
        public string? HeroTitle { get; private set; }
        public string? HeroSubtitle { get; private set; }
        public string? HeroDescription { get; private set; }
        public string? HeroBackgroundImage { get; private set; }
        public string? HeroButtonText { get; private set; }
        public string? HeroButtonUrl { get; private set; }

        // SEO
        public string? MetaTitle { get; private set; }
        public string? MetaDescription { get; private set; }
        public string? MetaKeywords { get; private set; }

        private Page() { }

        public static Page Create(string title, PageType pageType)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            return new Page
            {
                Title = title,
                Slug = SlugGenerator.Generate(title), // base slug; app layer will replace with unique one
                PageType = pageType,
                IsActive = true
            };
        }

        // Main update method (does NOT touch Slug)
        public void Update(string title, PageType pageType, string? content)
        {
            Title = title;
            PageType = pageType;
            Content = content;
            AddDomainEvent(new PageUpdatedEvent(this.Id));
        }

        public void UpdateHeroSection(
            string? heroTitle,
            string? heroSubtitle,
            string? heroDescription,
            string? heroBackgroundImage,
            string? heroButtonText,
            string? heroButtonUrl)
        {
            HeroTitle = heroTitle;
            HeroSubtitle = heroSubtitle;
            HeroDescription = heroDescription;
            HeroBackgroundImage = heroBackgroundImage;
            HeroButtonText = heroButtonText;
            HeroButtonUrl = heroButtonUrl;
            AddDomainEvent(new PageUpdatedEvent(this.Id));
        }

        public void UpdateSeo(string? metaTitle, string? metaDescription, string? metaKeywords)
        {
            MetaTitle = metaTitle;
            MetaDescription = metaDescription;
            MetaKeywords = metaKeywords;
            AddDomainEvent(new PageUpdatedEvent(this.Id));
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            AddDomainEvent(new PageUpdatedEvent(this.Id));
        }
        
        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public void UpdateSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug is required.", nameof(slug));
            Slug = slug;
        }

        private static string GenerateSlug(string title)
            => title.ToLower().Replace(" ", "-"); // keep simple; app layer sanitizes/uniquifies
    }
}
