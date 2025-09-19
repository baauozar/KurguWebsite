using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendContactFormEmailAsync(string name, string email, string subject, string message);
        Task<bool> SendTemplateEmailAsync(string to, string templateName, object model);
    }
}
