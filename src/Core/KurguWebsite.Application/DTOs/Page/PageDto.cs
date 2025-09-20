using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Page
{
    public class PageDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string PageType { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Hero Section
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? HeroDescription { get; set; }
        public string? HeroBackgroundImage { get; set; }
        public string? HeroButtonText { get; set; }
        public string? HeroButtonUrl { get; set; }

        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }
}
