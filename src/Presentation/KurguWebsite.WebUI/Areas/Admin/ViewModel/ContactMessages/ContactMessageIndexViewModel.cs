using KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin;

namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.ContactMessages
{
    public class ContactMessageIndexViewModel : PagedViewModel<ContactMessageListItemViewModel>
    {
        public int UnreadCount { get; set; }
        public int UnrepliedCount { get; set; }
    }

    public class ContactMessageListItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsReplied { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
