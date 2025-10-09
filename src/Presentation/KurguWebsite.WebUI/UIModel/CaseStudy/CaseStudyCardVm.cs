using System.Globalization;

namespace KurguWebsite.WebUI.UIModel.CaseStudy
{
    public class CaseStudyCardVm
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public string Client { get; init; } = "";
        public string Summary { get; init; } = "";
        public string ThumbnailUrl { get; init; } = "";
        public string Slug { get; init; } = "";
        public DateTime CompletedDate { get; set; }

        public string CompletedDateText =>
       CompletedDate.ToString("dd MMM yyyy", CultureInfo.CurrentCulture);

    }
}

