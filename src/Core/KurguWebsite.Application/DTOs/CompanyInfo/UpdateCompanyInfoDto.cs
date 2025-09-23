using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.CompanyInfo
{
    public class UpdateCompanyInfoDto
    {
        public string? CompanyName { get; set; }
        public string? About { get; set; }
        public string? Mission { get; set; }
        public string? Vision { get; set; }
        public string? Slogan { get; set; }
        public string? CopyrightText { get; set; }
        public string? LogoPath { get; set; }
        public string? LogoLightPath { get; set; }
    }
}
