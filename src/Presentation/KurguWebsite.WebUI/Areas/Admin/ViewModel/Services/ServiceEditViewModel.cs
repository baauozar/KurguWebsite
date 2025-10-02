namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Services
{
    public class ServiceEditViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string? FullDescription { get; set; }
        public string? IconFile { get; set; }
        public string IconPath { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int Category { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }

        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }

        // Features
        public List<ServiceFeatureEditViewModel> Features { get; set; } = new();
    }

    public class ServiceFeatureEditViewModel
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
    }
}
