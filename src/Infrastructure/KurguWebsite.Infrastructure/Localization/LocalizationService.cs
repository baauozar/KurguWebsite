using KurguWebsite.Application.Common.Interfaces;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;


namespace KurguWebsite.Infrastructure.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer _localizer;
        private readonly LocalizationOptions _options;
        private CultureInfo _currentCulture;

        public LocalizationService(
            IStringLocalizerFactory factory,
            IOptions<LocalizationOptions> options)
        {
            _localizer = factory.Create("SharedResources", "KurguWebsite.Infrastructure");
            _options = options.Value;
            _currentCulture = CultureInfo.CurrentCulture;
        }

        public CultureInfo CurrentCulture => _currentCulture;

        public List<CultureInfo> SupportedCultures => _options.SupportedCultures;

        public string GetLocalizedString(string key)
        {
            return _localizer[key];
        }

        public string GetLocalizedString(string key, string culture)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var localizedString = _localizer[key];

            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousCulture;

            return localizedString;
        }

        public async Task<Dictionary<string, string>> GetAllStringsAsync(string culture)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var allStrings = _localizer.GetAllStrings()
                .ToDictionary(x => x.Name, x => x.Value);

            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousCulture;

            return await Task.FromResult(allStrings);
        }

        public void SetCulture(string culture)
        {
            _currentCulture = new CultureInfo(culture);
            CultureInfo.CurrentCulture = _currentCulture;
            CultureInfo.CurrentUICulture = _currentCulture;
        }
    }

    public class LocalizationOptions
    {
        public List<CultureInfo> SupportedCultures { get; set; } = new List<CultureInfo>
        {
            new CultureInfo("en-US"),
            new CultureInfo("tr-TR")
        };

        public string DefaultCulture { get; set; } = "en-US";
    }
}