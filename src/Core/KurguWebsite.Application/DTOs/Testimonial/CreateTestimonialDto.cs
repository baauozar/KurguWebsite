using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Testimonial
{
    public class CreateTestimonialDto
    {
        public string ClientName { get; set; } = string.Empty;
        public string ClientTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ClientImagePath { get; set; }
        public int Rating { get; set; } = 5;
        public bool IsFeatured { get; set; }
    }
}
