// src/Core/KurguWebsite.Domain/Specifications/ContactMessageSpecifications.cs
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Domain.Specifications
{
    public class UnreadMessagesSpecification : BaseSpecification<ContactMessage>
    {
        public UnreadMessagesSpecification()
            : base(cm => !cm.IsRead && !cm.IsDeleted)
        {
            ApplyOrderByDescending(cm => cm.CreatedDate);
        }
    }

    public class UnrepliedMessagesSpecification : BaseSpecification<ContactMessage>
    {
        public UnrepliedMessagesSpecification()
            : base(cm => !cm.IsReplied && !cm.IsDeleted)
        {
            ApplyOrderByDescending(cm => cm.CreatedDate);
        }
    }

    public class MessagesByDateRangeSpecification : BaseSpecification<ContactMessage>
    {
        public MessagesByDateRangeSpecification(DateTime startDate, DateTime endDate)
            : base(cm =>
                !cm.IsDeleted &&
                cm.CreatedDate >= startDate &&
                cm.CreatedDate <= endDate)
        {
            ApplyOrderByDescending(cm => cm.CreatedDate);
        }
    }

    public class MessageSearchSpecification : BaseSpecification<ContactMessage>
    {
        public MessageSearchSpecification(
            string? searchTerm,
            bool? isRead,
            bool? isReplied,
            int pageNumber,
            int pageSize)
            : base(cm =>
                !cm.IsDeleted &&
                (isRead == null || cm.IsRead == isRead) &&
                (isReplied == null || cm.IsReplied == isReplied) &&
                (string.IsNullOrEmpty(searchTerm) ||
                 cm.Name.Contains(searchTerm) ||
                 cm.Email.Contains(searchTerm) ||
                 cm.Subject.Contains(searchTerm) ||
                 cm.Message.Contains(searchTerm)))
        {
            ApplyOrderByDescending(cm => cm.CreatedDate);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    public class PriorityMessagesSpecification : BaseSpecification<ContactMessage>
    {
        public PriorityMessagesSpecification()
            : base(cm =>
                !cm.IsDeleted &&
                !cm.IsReplied &&
                (cm.Subject.ToLower().Contains("urgent") ||
                 cm.Subject.ToLower().Contains("important") ||
                 cm.Message.ToLower().Contains("urgent")))
        {
            ApplyOrderByDescending(cm => cm.CreatedDate);
        }
    }
}