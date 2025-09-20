using KurguWebsite.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace KurguWebsite.Infrastructure.Services.Email
{
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
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:Username"];
            _smtpPassword = _configuration["Email:Password"];
            _fromEmail = _configuration["Email:FromEmail"];
            _fromName = _configuration["Email:FromName"];
        }

        // ✅ Core method using EmailMessage
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

                if (message.Cc?.Any() == true)
                    foreach (var cc in message.Cc)
                        mailMessage.CC.Add(cc);

                if (message.Bcc?.Any() == true)
                    foreach (var bcc in message.Bcc)
                        mailMessage.Bcc.Add(bcc);

                if (message.Attachments?.Any() == true)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        var stream = new MemoryStream(attachment.Content);
                        mailMessage.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
                    }
                }

                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // TODO: log error properly
                return false;
            }
        }

        // ✅ Template emails
        public async Task<bool> SendTemplateEmailAsync(string templateName, string to, object data)
        {
            var template = await LoadEmailTemplate(templateName);
            var body = ProcessTemplate(template, data);
            var subject = GetTemplateSubject(templateName);

            var message = new EmailMessage
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message);
        }

        private async Task<string> LoadEmailTemplate(string templateName)
        {
            var templatePath = Path.Combine("EmailTemplates", $"{templateName}.html");
            if (!File.Exists(templatePath)) return string.Empty;
            return await File.ReadAllTextAsync(templatePath);
        }

        private string ProcessTemplate(string template, object data)
        {
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
                "ContactAutoReply" => "Thank you for contacting us",
                _ => "Kurgu Website Notification"
            };
        }
    }
}
