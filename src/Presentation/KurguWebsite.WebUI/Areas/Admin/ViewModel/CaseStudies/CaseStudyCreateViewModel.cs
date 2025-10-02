namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.CaseStudies
{
    public class CaseStudyCreateViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Challenge { get; set; }
        public string? Solution { get; set; }
        public string? Result { get; set; }
        public string? ImageFile { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CompletedDate { get; set; } = DateTime.Now;
        public string? Industry { get; set; }
        public Guid? ServiceId { get; set; }
        public bool IsFeatured { get; set; }

        // Technologies (comma-separated or list)
        public string? TechnologiesText { get; set; }
        public List<string> Technologies { get; set; } = new();

        // Available services for dropdown
        public List<ServiceDropdownViewModel> AvailableServices { get; set; } = new();

        // Metrics
        public List<CaseStudyMetricCreateViewModel> Metrics { get; set; } = new();
    }

    public class CaseStudyMetricCreateViewModel
    {
        public string MetricName { get; set; } = string.Empty;
        public string MetricValue { get; set; } = string.Empty;
        public string? Icon { get; set; }
    }
}
