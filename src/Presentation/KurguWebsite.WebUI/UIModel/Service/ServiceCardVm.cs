namespace KurguWebsite.WebUI.UIModel.Service
{
    public class ServiceCardVm
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public string ShortDescription { get; init; } = "";
        public string IconClass { get; init; } = ""; // e.g., "icon-layers" from Template
        public string Slug { get; init; } = "";
    }
}
