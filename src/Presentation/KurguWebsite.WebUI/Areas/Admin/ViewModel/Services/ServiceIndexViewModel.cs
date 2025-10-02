using KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin;

namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Services
{
    public class ServiceIndexViewModel : PagedViewModel<ServiceListItemViewModel>
    {
        public List<string> Categories { get; set; } = new();
        public string? SelectedCategory { get; set; }
    }

    public class ServiceListItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
