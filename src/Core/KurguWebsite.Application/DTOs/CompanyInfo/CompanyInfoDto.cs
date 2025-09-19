using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Service
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
        public string SupportPhone { get; set; } = string.Empty;
        public string SalesPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SupportEmail { get; set; }
        public string? SalesEmail { get; set; }

        // Address
        public string Street { get; set; } = string.Empty;
        public string? Suite { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;

        // Social Media
        public string? Facebook { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Instagram { get; set; }
        public string? YouTube { get; set; }
    }
}
