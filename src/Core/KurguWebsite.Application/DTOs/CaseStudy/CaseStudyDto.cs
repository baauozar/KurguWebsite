using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Service
{
    public class CaseStudyDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public DateTime CompletedDate { get; set; }
        public string? Industry { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public Guid? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public List<string> Technologies { get; set; } = new();
    }
}
