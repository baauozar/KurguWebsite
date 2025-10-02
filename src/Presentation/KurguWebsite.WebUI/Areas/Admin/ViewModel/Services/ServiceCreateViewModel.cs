namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Services
{
    public class ServiceCreateViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string? FullDescription { get; set; }
        public string? IconFile { get; set; }
        public string? IconPath { get; set; }
        public string? IconClass { get; set; }
        public int Category { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }

        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }

        // Features
        public List<ServiceFeatureCreateViewModel> Features { get; set; } = new();
    }

    public class ServiceFeatureCreateViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
    }
}
