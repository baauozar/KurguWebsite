using KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin;

namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.CaseStudies
{
    public class CaseStudyIndexViewModel : PagedViewModel<CaseStudyListItemViewModel>
    {
        public List<ServiceDropdownViewModel> Services { get; set; } = new();
        public Guid? SelectedServiceId { get; set; }
    }

    public class CaseStudyListItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string? ServiceName { get; set; }
        public DateTime CompletedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
    }

    public class ServiceDropdownViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
