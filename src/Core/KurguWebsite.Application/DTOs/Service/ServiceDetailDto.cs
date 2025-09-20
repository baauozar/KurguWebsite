using KurguWebsite.Application.DTOs.CaseStudy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Service
{
    public class ServiceDetailDto : ServiceDto
    {
        public string? FullDescription { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public List<ServiceFeatureDto> Features { get; set; } = new();
        public List<CaseStudyDto> RelatedCaseStudies { get; set; } = new();
        public List<ServiceDto> OtherServices { get; set; } = new();
    }
}
