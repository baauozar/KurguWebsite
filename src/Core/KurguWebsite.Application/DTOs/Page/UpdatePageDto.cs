using KurguWebsite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Page
{
    public class UpdatePageDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public PageType? PageType { get; set; }
        public bool? IsActive { get; set; }

        // Hero Section
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? HeroDescription { get; set; }
        public string? HeroBackgroundImage { get; set; }
        public string? HeroButtonText { get; set; }
        public string? HeroButtonUrl { get; set; }

        // SEO
        public string? ContentImagePath { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }
}
