using KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin;

namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Testimonials
{
    public class TestimonialIndexViewModel : PagedViewModel<TestimonialListItemViewModel>
    {
    }

    public class TestimonialListItemViewModel
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
    }
}
