using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.CompanyInfo
{
    public class CompanyInfoDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public string? LogoLightPath { get; set; }
        public string? About { get; set; }
        public string? Mission { get; set; }
        public string? Vision { get; set; }
        public string? Slogan { get; set; }
        public string? CopyrightText { get; set; }

        // Contact Info
        public ContactInfoDto ContactInformation { get; set; } = new();

        // Address
        public AddressDto OfficeAddress { get; set; } = new();

        // Social Media
        public SocialMediaDto? SocialMedia { get; set; }

        // Simplified access
        public string Email => ContactInformation?.Email ?? string.Empty;
        public string Phone => ContactInformation?.SupportPhone ?? string.Empty;
    }
}
