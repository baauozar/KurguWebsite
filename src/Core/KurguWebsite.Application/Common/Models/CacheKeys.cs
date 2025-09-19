using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    public static class CacheKeys
    {
        public const string Services = "services";
        public const string FeaturedServices = "featured_services";
        public const string ActiveServices = "active_services";
        public const string ServiceBySlug = "service_slug_{0}";
        public const string ServiceById = "service_id_{0}";

        public const string CaseStudies = "case_studies";
        public const string FeaturedCaseStudies = "featured_case_studies";

        public const string Testimonials = "testimonials";
        public const string ActiveTestimonials = "active_testimonials";

        public const string Partners = "partners";
        public const string ActivePartners = "active_partners";

        public const string CompanyInfo = "company_info";

        public const string HomePage = "home_page";
        public const string AboutPage = "about_page";
        public const string ServicesPage = "services_page";
        public const string ContactPage = "contact_page";

        public const string ProcessSteps = "process_steps";
    }
}
