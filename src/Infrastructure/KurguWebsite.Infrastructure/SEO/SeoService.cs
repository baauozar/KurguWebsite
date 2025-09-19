using KurguWebsite.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace KurguWebsite.Infrastructure.SEO
{
    public class SeoService : ISeoService
    {
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

            slug = slug.ToLowerInvariant()
                       .Replace("ç", "c").Replace("ş", "s").Replace("ğ", "g")
                       .Replace("ü", "u").Replace("ö", "o").Replace("ı", "i").Replace("İ", "i");

            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
            return slug;
        }

        public bool IsSlugValid(string slug)
        {
            return !string.IsNullOrWhiteSpace(slug) &&
                   Regex.IsMatch(slug, @"^[a-z0-9]+(-[a-z0-9]+)*$");
        }

        public SeoMetadata GenerateMetadata(string title, string description, string keywords = null)
        {
            title = title?.Trim() ?? string.Empty;
            description = description?.Trim() ?? string.Empty;

            return new SeoMetadata
            {
                Title = title.Length > 60 ? title.Substring(0, 57) + "..." : title,
                Description = description.Length > 160 ? description.Substring(0, 157) + "..." : description,
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
