using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Service
{
    public class HomePageDto
    {
        public PageDto PageInfo { get; set; } = null!;
        public List<ServiceDto> FeaturedServices { get; set; } = new();
        public List<CaseStudyDto> FeaturedCaseStudies { get; set; } = new();
        public TestimonialDto? FeaturedTestimonial { get; set; }
        public List<ProcessStepDto> ProcessSteps { get; set; } = new();
        public List<PartnerDto> Partners { get; set; } = new();
        public CompanyInfoDto CompanyInfo { get; set; } = null!;
    }
}
