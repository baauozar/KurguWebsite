namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Testimonials
{
    public class TestimonialCreateViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string ClientTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ClientImageFile { get; set; }
        public string? ClientImagePath { get; set; }
        public int Rating { get; set; } = 5;
        public DateTime TestimonialDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
    }
}
