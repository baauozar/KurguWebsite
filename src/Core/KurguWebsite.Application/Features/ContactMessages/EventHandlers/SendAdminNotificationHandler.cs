using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using KurguWebsite.Application.Common.Interfaces; // For IEmailService

namespace KurguWebsite.Application.Features.ContactMessages.EventHandlers
{
    public class SendAdminNotificationHandler : INotificationHandler<ContactMessageReceivedEvent>
    {
        private readonly ILogger<SendAdminNotificationHandler> _logger;
         private readonly IEmailService _emailService;

        public SendAdminNotificationHandler(ILogger<SendAdminNotificationHandler> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public Task Handle(ContactMessageReceivedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("WORKFLOW: New contact message from {Name} ({Email}). Triggering admin notification.",
                notification.Message.Name, notification.Message.Email);

            // You would add your email/SignalR logic here.
            // await _emailService.SendAsync("admin@yourdomain.com", "New Contact Form Submission", $"...body...");

            return Task.CompletedTask;
        }
    }
}