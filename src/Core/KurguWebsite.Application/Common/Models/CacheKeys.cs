using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    public static class CacheKeys
    {
        public const string Services = "services_all";
        public const string FeaturedServices = "services_featured";
        public const string ServiceById = "service_id_{0}";
        public const string ServiceBySlug = "service_slug_{0}";
        public const string CaseStudies = "casestudies_all";
        public const string FeaturedCaseStudies = "casestudies_featured";
        public const string CaseStudyBySlug = "casestudy_slug_{0}";
        public const string Testimonials = "testimonials_all";
        public const string ActiveTestimonials = "testimonials_active";
        public const string Partners = "partners_all";
        public const string ActivePartners = "partners_active";
        public const string Pages = "pages_all";
        public const string PageBySlug = "page_slug_{0}";
        public const string ProcessSteps = "processsteps_all";
        public const string CompanyInfo = "companyinfo";
        public const string HomePage = "homepage_data";
        public const string AboutPage = "aboutpage_data";
        public const string ContactPage = "contactpage_data";
        public const string ServicesPage = "servicespage_data";
    }
}
