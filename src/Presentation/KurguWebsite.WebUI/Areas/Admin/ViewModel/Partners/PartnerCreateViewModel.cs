namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Partners
{
    public class PartnerCreateViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoFile { get; set; }
        public string? LogoPath { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public int DisplayOrder { get; set; }
    }
}
