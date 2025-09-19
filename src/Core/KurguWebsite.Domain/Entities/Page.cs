using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class Page : AuditableEntity, ISeoEntity
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
            return new Page
            {
                Title = title,
                Slug = GenerateSlug(title),
                PageType = pageType,
                IsActive = true
            };
        }

        public void UpdateContent(string? content)
        {
            Content = content;
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
        }

        public void UpdateSeo(string? metaTitle, string? metaDescription, string? metaKeywords)
        {
            MetaTitle = metaTitle;
            MetaDescription = metaDescription;
            MetaKeywords = metaKeywords;
        }

        private static string GenerateSlug(string title)
        {
            return title.ToLower().Replace(" ", "-");
        }
    }
}