using KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin;

namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.CaseStudies
{
    public class CaseStudyDetailsViewModel
    {
        public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public DateTime CompletedDate { get; set; }
    public string? Industry { get; set; }
    public string? ServiceName { get; set; }
    public List<string> Technologies { get; set; } = new();
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public List<CaseStudyMetricViewModel> Metrics { get; set; } = new();
}

public class CaseStudyMetricViewModel
{
    public Guid Id { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string MetricValue { get; set; } = string.Empty;
    public string? Icon { get; set; }
}
}
