using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.DTOs.Testimonial;

namespace KurguWebsite.WebUI.UIModel.Home
{
    public class HomeViewModel
    {
        public List<ServiceDto> FeaturedServices { get; set; } = new();
        public List<TestimonialDto> Testimonials { get; set; } = new();
        public List<PartnerDto> Partners { get; set; } = new();
        public List<CaseStudyDto> RecentCaseStudies { get; set; } = new();
    }
}
