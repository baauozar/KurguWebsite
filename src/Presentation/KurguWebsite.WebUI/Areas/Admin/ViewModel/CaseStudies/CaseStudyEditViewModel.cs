namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.CaseStudies
{
    public class CaseStudyEditViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Challenge { get; set; }
        public string? Solution { get; set; }
        public string? Result { get; set; }
        public string? ImageFile { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CompletedDate { get; set; }
        public string? Industry { get; set; }
        public Guid? ServiceId { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }

        // Technologies
        public string? TechnologiesText { get; set; }
        public List<string> Technologies { get; set; } = new();

        // Available services
        public List<ServiceDropdownViewModel> AvailableServices { get; set; } = new();

        // Metrics
        public List<CaseStudyMetricEditViewModel> Metrics { get; set; } = new();
    }

    public class CaseStudyMetricEditViewModel
    {
        public Guid? Id { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public string MetricValue { get; set; } = string.Empty;
        public string? Icon { get; set; }
    }
}
