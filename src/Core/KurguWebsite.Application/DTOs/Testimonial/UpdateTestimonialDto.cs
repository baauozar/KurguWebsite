using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Testimonial
{
    public class UpdateTestimonialDto
    {
        public string? ClientName { get; set; }
        public string? ClientTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? Content { get; set; }
        public string? ClientImagePath { get; set; }
        public int? Rating { get; set; }
        public DateTime? TestimonialDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
