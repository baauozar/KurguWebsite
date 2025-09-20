using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    public static class CacheKeys
    {
        private const string Prefix = "kurgu:";
        public const string Services = Prefix +"services";
        public const string FeaturedServices = Prefix + "featured_services";
        public const string ActiveServices = Prefix + "active_services";
        public const string ServiceBySlug = Prefix + "service_slug_{0}";
        public const string ServiceById = Prefix + "service_id_{0}";

        public const string CaseStudies = Prefix + "case_studies";
        public const string FeaturedCaseStudies = Prefix + "featured_case_studies";

        public const string Testimonials = Prefix + "testimonials";
        public const string ActiveTestimonials = Prefix + "active_testimonials";

        public const string Partners = Prefix + "partners";
        public const string ActivePartners = Prefix + "active_partners";

        public const string CompanyInfo = Prefix + "company_info";

        public const string HomePage = Prefix + "home_page";
        public const string AboutPage = Prefix + "about_page";
        public const string ServicesPage = Prefix + "services_page";
        public const string ContactPage = Prefix + "contact_page";

        public const string ProcessSteps = Prefix + "process_steps";
    }
}
