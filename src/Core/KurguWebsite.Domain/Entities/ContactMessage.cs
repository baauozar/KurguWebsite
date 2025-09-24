using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class ContactMessage : AuditableEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public string Subject { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }
        public bool IsReplied { get; private set; }
        public DateTime? RepliedDate { get; private set; }
        public string? RepliedBy { get; private set; }

        private ContactMessage() { }

        public static ContactMessage Create(
            string name,
            string email,
            string phone,
            string subject,
            string message)
        {
            return new ContactMessage
            {
                Name = name,
                Email = email,
                Phone = phone,
                Subject = subject,
                Message = message,
                IsRead = false,
                IsReplied = false
            };
        }

        public void MarkAsRead() => IsRead = true;

        public void MarkAsReplied(string userId)
        {
            IsReplied = true;
            RepliedDate = DateTime.UtcNow;
            RepliedBy = userId;
        }
    }
}