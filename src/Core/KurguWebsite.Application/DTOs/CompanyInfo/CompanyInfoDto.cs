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

        // NEW: Statistics for About page counters
        public int YearsInBusiness { get; set; }
        public int TotalClients { get; set; }
        public int ProjectsCompleted { get; set; }
        public int TeamMembers { get; set; }

        // NEW: Section images for Mission/Vision/Careers cards
        public string? MissionImagePath { get; set; }
        public string? VisionImagePath { get; set; }
        public string? CareersImagePath { get; set; }

        // NEW: Careers section content
        public string? CareersDescription { get; set; }

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
