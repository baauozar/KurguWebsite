using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Interfaces
{
    public interface ILocalizationService
    {
        string GetLocalizedString(string key);
        string GetLocalizedString(string key, string culture);
        Task<Dictionary<string, string>> GetAllStringsAsync(string culture);
        CultureInfo CurrentCulture { get; }
        List<CultureInfo> SupportedCultures { get; }
        void SetCulture(string culture);
    }
}
