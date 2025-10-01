using KurguWebsite.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace KurguWebsite.Infrastructure.SEO
{
    public class SeoService : ISeoService
    {
        private readonly IConfiguration _configuration;

        public SeoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // English stopwords
            "and","or","but","with","the","a","an","for","of","in","on","at",
            "to","from","by","is","are","was","were","be","as","that","this",
            "it","its","which","into","over","under","up","down","out","about",
            // Turkish stopwords
            "ve","ile","ama","bir","bu","o","de","da","mi","mı","mu","mü",
            "için","kadar","gibi","ne","ki","şu","her","çok","az","var","yok"
        };

        public string GenerateSlug(string title) => SanitizeSlug(title);

        public string SanitizeSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return string.Empty;

            // Normalize & Turkish fold first (upper/lower variants)
            slug = slug.Normalize(NormalizationForm.FormD);

            // Turkish-specific letter mapping first (handles dotted/undotted i correctly)
            slug = slug
                .Replace("İ", "I").Replace("İ", "I")  // uppercase dotted I to I (rare edge)
                .Replace("ı", "i").Replace("İ", "I")
                .Replace("ş", "s").Replace("Ş", "S")
                .Replace("ğ", "g").Replace("Ğ", "G")
                .Replace("ç", "c").Replace("Ç", "C")
                .Replace("ö", "o").Replace("Ö", "O")
                .Replace("ü", "u").Replace("Ü", "U");

            // Remove combining marks (accents/diacritics)
            slug = new string(slug.Where(ch => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray())
                   .Normalize(NormalizationForm.FormC);

            // Lowercase invariant (after mapping)
            slug = slug.ToLowerInvariant();

            // Remove anything not alphanumeric/space/hyphen
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Collapse whitespace to single dashes
            slug = Regex.Replace(slug, @"\s+", "-");

            // Collapse multiple dashes
            slug = Regex.Replace(slug, @"-+", "-");

            // Trim dashes
            slug = slug.Trim('-');

            // Optional: hard cap length (keeps URLs short)
            if (slug.Length > 200) slug = slug[..200].Trim('-');

            return slug;
        }

        public bool IsSlugValid(string slug)
        {
            return !string.IsNullOrWhiteSpace(slug) &&
                   Regex.IsMatch(slug, @"^[a-z0-9]+(-[a-z0-9]+)*$");
        }

        public SeoMetadata GenerateMetadata(string title, string description, string? keywords = null)
        {
            title ??= string.Empty;
            description ??= string.Empty;

            var t = title.Length > 60 ? $"{title.AsSpan(0, 57)}..." : title;
            var d = description.Length > 160 ? $"{description.AsSpan(0, 157)}..." : description;

            return new SeoMetadata
            {
                Title = t,
                Description = d,
                Keywords = string.IsNullOrWhiteSpace(keywords)
                    ? GenerateMetaKeywords($"{title} {description}")
                    : keywords
            };
        }

        // Implement the interface method correctly
        public string GenerateMetaKeywords(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;

            var words = Regex.Replace(content.ToLowerInvariant(), @"[^a-z0-9ığüşöç\s]", "")
                             .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                             .Where(w => w.Length > 3 && !StopWords.Contains(w))
                             .GroupBy(w => w)
                             .OrderByDescending(g => g.Count())
                             .Take(15)
                             .Select(g => g.Key);

            return string.Join(", ", words);
        }

        public string GenerateMetaDescription(string content, int maxLength = 160)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;

            var plainText = Regex.Replace(content, "<.*?>", "").Trim();

            return plainText.Length > maxLength
                ? plainText.Substring(0, maxLength - 3).Trim() + "..."
                : plainText;
        }

        public string GenerateSitemap(IEnumerable<SitemapNode> nodes)
        {
            var sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "urlset",
                    nodes.Select(node =>
                        new XElement("url",
                            new XElement("loc", node.Url),
                            new XElement("lastmod", node.LastModified.ToString("yyyy-MM-dd")),
                            new XElement("changefreq", string.IsNullOrWhiteSpace(node.ChangeFrequency) ? "weekly" : node.ChangeFrequency),
                            new XElement("priority", node.Priority.ToString("F1"))
                        )
                    )
                )
            );

            return sitemap.ToString();
        }

        public string GenerateStructuredData(object data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            return $"<script type=\"application/ld+json\">{json}</script>";
        }

        public string GenerateCanonicalUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            try
            {
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);

                // If relative, prepend base domain (from config if needed)
                if (!uri.IsAbsoluteUri)
                {
                    // Example: load from configuration, otherwise fallback
                    var baseDomain = _configuration["Site:BaseUrl"] ?? "https://www.example.com";
                    uri = new Uri(new Uri(baseDomain), uri);
                }

                // Normalize: lowercase host, no trailing slash unless root
                var builder = new UriBuilder(uri)
                {
                    Host = uri.Host.ToLowerInvariant(),
                    Scheme = "https",
                    Port = -1 // remove port if default
                };

                var canonical = builder.Uri.ToString().TrimEnd('/');
                return canonical;
            }
            catch
            {
                return url; // fallback if parsing fails
            }
        }

        public string GenerateRobotsTxt(bool allowAll = true)
        {
            if (allowAll)
            {
                return
        @"User-agent: *
Allow: /";
            }
            else
            {
                return
        @"User-agent: *
Disallow: /";
            }
        }

        public string GenerateOpenGraphTags(OpenGraphData data)
        {
            if (data == null) return string.Empty;

            var tags = new List<string>();

            if (!string.IsNullOrWhiteSpace(data.Title))
                tags.Add($"<meta property=\"og:title\" content=\"{System.Net.WebUtility.HtmlEncode(data.Title)}\" />");

            if (!string.IsNullOrWhiteSpace(data.Description))
                tags.Add($"<meta property=\"og:description\" content=\"{System.Net.WebUtility.HtmlEncode(data.Description)}\" />");

            if (!string.IsNullOrWhiteSpace(data.Url))
                tags.Add($"<meta property=\"og:url\" content=\"{data.Url}\" />");

            if (!string.IsNullOrWhiteSpace(data.ImageUrl))
                tags.Add($"<meta property=\"og:image\" content=\"{data.ImageUrl}\" />");

            tags.Add($"<meta property=\"og:type\" content=\"{(string.IsNullOrWhiteSpace(data.Type) ? "website" : data.Type)}\" />");

            if (!string.IsNullOrWhiteSpace(data.SiteName))
                tags.Add($"<meta property=\"og:site_name\" content=\"{System.Net.WebUtility.HtmlEncode(data.SiteName)}\" />");

            return string.Join(Environment.NewLine, tags);
        }
        private static string RemoveStopWordsForSlug(string text)
        {
            var words = Regex.Replace(text, @"\s+", " ").Trim()
                             .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var filtered = words.Where(w => !StopWords.Contains(w, StringComparer.OrdinalIgnoreCase));
            var result = string.Join(' ', filtered);
            return string.IsNullOrWhiteSpace(result) ? text : result;
        }
    }

    public class SeoMetadata
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
    }

    public class SitemapNode
    {
        public string Url { get; set; }
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public string ChangeFrequency { get; set; } = "weekly";
        public double Priority { get; set; } = 0.5;
    }
}
