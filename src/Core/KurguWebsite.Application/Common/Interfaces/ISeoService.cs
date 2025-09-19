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
    }
}
