using KurguWebsite.Domain.Entities;
using System;

namespace KurguWebsite.Domain.Events
{
    /// <summary>
    /// Published when a new contact message is submitted.
    /// This can be used to trigger notifications (email, SignalR, etc.).
    /// </summary>
    public class ContactMessageReceivedEvent : GenericDomainEvent<ContactMessage>
    {
        public ContactMessage Message { get; }

        public ContactMessageReceivedEvent(ContactMessage message) : base(message.Id)
        {
            Message = message;
        }
    }
}