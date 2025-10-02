namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Partners
{
    public class PartnerEditViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoFile { get; set; }
        public string LogoPath { get; set; } = string.Empty;
        public string? WebsiteUrl { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
