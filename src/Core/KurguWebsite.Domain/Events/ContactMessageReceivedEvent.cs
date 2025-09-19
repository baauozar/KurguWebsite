using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Events
{
    public class ContactMessageReceivedEvent : DomainEvent
    {
        public string SenderName { get; }
        public string SenderEmail { get; }
        public string Subject { get; }

        public ContactMessageReceivedEvent(
            Guid messageId,
            string senderName,
            string senderEmail,
            string subject)
            : base(messageId)
        {
            SenderName = senderName;
            SenderEmail = senderEmail;
            Subject = subject;
        }
    }
}
