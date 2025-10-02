namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Services
{
    public class ServiceDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string? FullDescription { get; set; }
        public string IconPath { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string Category { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedDate { get; set; }

        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }

        // Features
        public List<ServiceFeatureViewModel> Features { get; set; } = new();

        // Related
        public List<RelatedCaseStudyViewModel> RelatedCaseStudies { get; set; } = new();
    }

    public class ServiceFeatureViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class RelatedCaseStudyViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
    }
}
