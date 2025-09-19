using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Service
{
    public class UpdateHeroSectionDto
    {
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? HeroDescription { get; set; }
        public string? HeroBackgroundImage { get; set; }
        public string? HeroButtonText { get; set; }
        public string? HeroButtonUrl { get; set; }
    }
}
