using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Infrastructure.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage message);
        Task<bool> SendTemplateEmailAsync(string templateName, string to, object data);
    }

    public class EmailMessage
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; } = true;
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public List<EmailAttachment> Attachments { get; set; } = new();
    }

    public class EmailAttachment
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            _smtpUsername = _configuration["Email:Username"];
            _smtpPassword = _configuration["Email:Password"];
            _fromEmail = _configuration["Email:FromEmail"];
            _fromName = _configuration["Email:FromName"];
        }

        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = message.Subject,
                    Body = message.Body,
                    IsBodyHtml = message.IsHtml
                };

                mailMessage.To.Add(message.To);

                foreach (var cc in message.Cc)
                    mailMessage.CC.Add(cc);

                foreach (var bcc in message.Bcc)
                    mailMessage.Bcc.Add(bcc);

                foreach (var attachment in message.Attachments)
                {
                    var stream = new MemoryStream(attachment.Content);
                    mailMessage.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
                }

                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch
            {
                // Log error
                return false;
            }
        }

        public async Task<bool> SendTemplateEmailAsync(string templateName, string to, object data)
        {
            // Load template and replace placeholders with data
            var template = await LoadEmailTemplate(templateName);
            var body = ProcessTemplate(template, data);

            return await SendEmailAsync(new EmailMessage
            {
                To = to,
                Subject = GetTemplateSubject(templateName),
                Body = body,
                IsHtml = true
            });
        }

        private async Task<string> LoadEmailTemplate(string templateName)
        {
            var templatePath = Path.Combine("EmailTemplates", $"{templateName}.html");
            return await File.ReadAllTextAsync(templatePath);
        }

        private string ProcessTemplate(string template, object data)
        {
            // Simple template processing - in production use a template engine
            var properties = data.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(data)?.ToString() ?? string.Empty;
                template = template.Replace($"{{{{{prop.Name}}}}}", value);
            }
            return template;
        }

        private string GetTemplateSubject(string templateName)
        {
            return templateName switch
            {
                "Welcome" => "Welcome to Kurgu Website",
                "ResetPassword" => "Reset Your Password",
                "OrderConfirmation" => "Order Confirmation",
                _ => "Kurgu Website Notification"
            };
        }
    }
}