namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.AuditLogs
{
    public class RecentActivityViewModel
    {
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TimeAgo => GetTimeAgo();
        public string ActionIcon => GetActionIcon();
        public string ActionColor => GetActionColor();

        private string GetTimeAgo()
        {
            var timeSpan = DateTime.UtcNow - Timestamp;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";

            return Timestamp.ToString("MMM dd, yyyy");
        }

        private string GetActionIcon()
        {
            return Action.ToLower() switch
            {
                var a when a.Contains("create") => "fas fa-plus-circle",
                var a when a.Contains("update") || a.Contains("edit") => "fas fa-edit",
                var a when a.Contains("delete") => "fas fa-trash-alt",
                var a when a.Contains("restore") => "fas fa-undo",
                _ => "fas fa-info-circle"
            };
        }

        private string GetActionColor()
        {
            return Action.ToLower() switch
            {
                var a when a.Contains("create") => "text-success",
                var a when a.Contains("update") || a.Contains("edit") => "text-info",
                var a when a.Contains("delete") => "text-danger",
                var a when a.Contains("restore") => "text-warning",
                _ => "text-secondary"
            };
        }
    }
}
