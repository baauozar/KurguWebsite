namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.AuditLogs
{
    public class AuditLogDetailsViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        // Parsed values for display
        public Dictionary<string, object>? OldValuesDict { get; set; }
        public Dictionary<string, object>? NewValuesDict { get; set; }
    }
}
