using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Partner
{
    public class PartnerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public string? WebsiteUrl { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
