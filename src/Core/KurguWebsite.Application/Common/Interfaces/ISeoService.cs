using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Interfaces
{
    public interface ISeoService
    {
        string GenerateSlug(string title);
        string GenerateMetaDescription(string content, int maxLength = 160);
        string GenerateMetaKeywords(string content);
        bool IsSlugValid(string slug);
        string SanitizeSlug(string slug);
        string GenerateCanonicalUrl(string url);
        string GenerateRobotsTxt(bool allowAll = true);
        string GenerateOpenGraphTags(OpenGraphData data);
    }
    public class OpenGraphData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Type { get; set; } = "website";
        public string SiteName { get; set; } = string.Empty;
    }
}
