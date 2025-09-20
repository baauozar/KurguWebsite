using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.CaseStudy
{
    public class UpdateCaseStudyDto
    {
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Challenge { get; set; }
        public string? Solution { get; set; }
        public string? Result { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CompletedDate { get; set; }
        public string? Industry { get; set; }
        public Guid? ServiceId { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
    }
}
