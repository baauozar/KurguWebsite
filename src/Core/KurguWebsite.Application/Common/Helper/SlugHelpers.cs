// src/Core/KurguWebsite.Application/Common/Helper/SlugHelpers.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Helper
{
    public static class SlugHelpers
    {
        /// <summary>
        /// Generate a unique slug for Services
        /// </summary>
        public static async Task<string> GenerateUniqueSlugForServiceAsync(
            IServiceUniquenessChecker unique,
            string title,
            Guid? excludeId,
            CancellationToken ct)
        {
            var baseSlug = SlugGenerator.Generate(title);
            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "service";

            var candidate = baseSlug;
            var index = 2;

            while (await unique.SlugExistsAsync(candidate, excludeId, ct))
            {
                candidate = $"{baseSlug}-{index++}";
            }

            return candidate;
        }

        /// <summary>
        /// Generate a unique slug for Case Studies
        /// </summary>
        public static async Task<string> GenerateUniqueSlugForCaseStudyAsync(
            ICaseStudyUniquenessChecker unique,
            string title,
            Guid? excludeId,
            CancellationToken ct)
        {
            var baseSlug = SlugGenerator.Generate(title);
            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "case-study";

            var candidate = baseSlug;
            var index = 2;

            while (await unique.SlugExistsAsync(candidate, excludeId, ct))
            {
                candidate = $"{baseSlug}-{index++}";
            }

            return candidate;
        }

        /// <summary>
        /// Generate a unique slug for Pages
        /// </summary>
        public static async Task<string> GenerateUniqueSlugForPageAsync(
            IPageUniquenessChecker unique,
            string title,
            Guid? excludeId,
            CancellationToken ct)
        {
            var baseSlug = SlugGenerator.Generate(title);
            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "page";

            var candidate = baseSlug;
            var index = 2;

            while (await unique.PageSlugExistsAsync(candidate, excludeId, ct))
            {
                candidate = $"{baseSlug}-{index++}";
            }

            return candidate;
        }

        /// <summary>
        /// Generic slug generator - works with any uniqueness checker
        /// </summary>
        public static async Task<string> GenerateUniqueSlugAsync<TChecker>(
            TChecker uniquenessChecker,
            string title,
            Guid? excludeId,
            string defaultSlug,
            Func<TChecker, string, Guid?, CancellationToken, Task<bool>> slugExistsFunc,
            CancellationToken ct)
        {
            var baseSlug = SlugGenerator.Generate(title);
            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = defaultSlug;

            var candidate = baseSlug;
            var index = 2;

            while (await slugExistsFunc(uniquenessChecker, candidate, excludeId, ct))
            {
                candidate = $"{baseSlug}-{index++}";
            }

            return candidate;
        }

        /// <summary>
        /// Generate slug without uniqueness check (used when uniqueness is guaranteed elsewhere)
        /// </summary>
        public static string GenerateSlug(string title, string defaultSlug = "item")
        {
            var slug = SlugGenerator.Generate(title);
            return string.IsNullOrWhiteSpace(slug) ? defaultSlug : slug;
        }

        /// <summary>
        /// Sanitize an existing slug
        /// </summary>
        public static string SanitizeSlug(string slug)
        {
            return SlugGenerator.Generate(slug);
        }
    }
}