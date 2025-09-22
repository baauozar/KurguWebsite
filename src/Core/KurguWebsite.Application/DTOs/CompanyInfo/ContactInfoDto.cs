using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.CompanyInfo
{
    public class ContactInfoDto
    {
        public string SupportPhone { get; set; } = string.Empty;
        public string SalesPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SupportEmail { get; set; }
        public string? SalesEmail { get; set; }
    }
}
