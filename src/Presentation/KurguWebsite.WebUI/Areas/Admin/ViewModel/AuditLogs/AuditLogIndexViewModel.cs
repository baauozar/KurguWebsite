using KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin;

namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.AuditLogs
{
    public class AuditLogIndexViewModel : PagedViewModel<AuditLogListItemViewModel>
    {
        public List<string> EntityTypes { get; set; } = new();
        public List<string> Actions { get; set; } = new();

        // Filters
        public string? SelectedEntityType { get; set; }
        public string? SelectedAction { get; set; }
        public string? SelectedUserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class AuditLogListItemViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        // Display helpers
        public string ActionBadgeClass => GetActionBadgeClass();
        public string ActionIcon => GetActionIcon();

        private string GetActionBadgeClass()
        {
            return Action.ToLower() switch
            {
                var a when a.Contains("create") => "badge-success",
                var a when a.Contains("update") || a.Contains("edit") => "badge-info",
                var a when a.Contains("delete") => "badge-danger",
                var a when a.Contains("restore") => "badge-warning",
                _ => "badge-secondary"
            };
        }

        private string GetActionIcon()
        {
            return Action.ToLower() switch
            {
                var a when a.Contains("create") => "fa-plus",
                var a when a.Contains("update") || a.Contains("edit") => "fa-edit",
                var a when a.Contains("delete") => "fa-trash",
                var a when a.Contains("restore") => "fa-undo",
                var a when a.Contains("login") => "fa-sign-in-alt",
                var a when a.Contains("logout") => "fa-sign-out-alt",
                _ => "fa-info-circle"
            };
        }
    }
}
