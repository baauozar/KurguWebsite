using KurguWebsite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Service
{
    public class UpdateServiceDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string? FullDescription { get; set; }
        public string IconPath { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public ServiceCategory Category { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }

        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }
}
