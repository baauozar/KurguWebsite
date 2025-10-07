using KurguWebsite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Partner
{
    public class CreatePartnerDto
    {
        public string Name { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public string? WebsiteUrl { get; set; }
        public string? Description { get; set; }
        public PartnerType Type { get; set; }
 
    }
}
