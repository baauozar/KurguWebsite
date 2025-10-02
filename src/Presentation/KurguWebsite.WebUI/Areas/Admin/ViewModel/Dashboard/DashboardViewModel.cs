using KurguWebsite.WebUI.Areas.Admin.ViewModel.AuditLogs;

namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.Dashboard
{
    public class DashboardViewModel
    {
        public DashboardStatistics Statistics { get; set; } = new();
        public List<RecentContactMessage> RecentMessages { get; set; } = new();
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    public class DashboardStatistics
    {
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public int TotalCaseStudies { get; set; }
        public int FeaturedCaseStudies { get; set; }
        public int TotalTestimonials { get; set; }
        public int TotalPartners { get; set; }
        public int UnreadMessages { get; set; }
        public int UnrepliedMessages { get; set; }
    }

    public class RecentContactMessage
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }
}