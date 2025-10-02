using KurguWebsite.Domain.Enums;

namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Pages
{
    public class PageEditViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public PageType PageType { get; set; }

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
