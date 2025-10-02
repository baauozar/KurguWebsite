namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin
{
    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsActive { get; set; }
    }

    public class BreadcrumbViewModel
    {
        public List<BreadcrumbItem> Items { get; set; } = new();
    }
}
