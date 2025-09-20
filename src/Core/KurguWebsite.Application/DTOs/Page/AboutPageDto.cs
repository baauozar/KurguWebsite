using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Application.DTOs.Testimonial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Page
{
    public class AboutPageDto
    {
        public PageDto PageInfo { get; set; } = null!;
        public CompanyInfoDto CompanyInfo { get; set; } = null!;
        public List<TestimonialDto> Testimonials { get; set; } = new();
        public List<PartnerDto> Partners { get; set; } = new();
    }
}
