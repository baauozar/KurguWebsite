using KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin;

namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Partners
{
    public class PartnerIndexViewModel : PagedViewModel<PartnerListItemViewModel>
    {
    }

    public class PartnerListItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public string? WebsiteUrl { get; set; }
        public string Type { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

}
