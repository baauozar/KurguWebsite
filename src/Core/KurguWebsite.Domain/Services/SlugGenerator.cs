using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Services
{
    public static class SlugGenerator
    {
        public static string Generate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to lowercase
            var slug = input.ToLowerInvariant();

            // Remove accents/diacritics
            slug = RemoveDiacritics(slug);

            // Replace invalid chars with hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Replace multiple spaces/hyphens with single hyphen
            slug = Regex.Replace(slug, @"[\s-]+", "-");

            // Trim hyphens from ends
            slug = slug.Trim('-');

            // Limit length
            if (slug.Length > 100)
                slug = slug.Substring(0, 100).TrimEnd('-');

            return slug;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}