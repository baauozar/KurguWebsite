using System.Globalization;

namespace KurguWebsite.WebUI.UIModel.CaseStudy
{
    public class CaseStudyDetailViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public DateTime CompletedDate { get; set; }
        public string? Industry { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public Guid? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public List<string> Technologies { get; set; } = new();

        // Metrics for the detail page
        public List<CaseStudyMetricVm> Metrics { get; set; } = new();

        // Convenience text for UI (same formatting you used on Services)
        public string CompletedDateText =>
            CompletedDate.ToString("dd MMM yyyy", CultureInfo.GetCultureInfo("tr-TR"));
        // (If you always want Turkish month names, use CultureInfo.GetCultureInfo("tr-TR") instead)
    }

    public class CaseStudyMetricVm
    {
        public string MetricName { get; set; } = string.Empty;
        public string MetricValue { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
    }

    // Optional: a compact card/list VM for index/related items

}