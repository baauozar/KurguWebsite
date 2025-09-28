using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Helper
{
    public static class SlugHelpers
    {
        public static async Task<string> GenerateUniqueSlugAsync(
            ISeoService seo,
            IServiceUniquenessChecker unique,
            string title,
            Guid? excludeId,
            CancellationToken ct)
        {
            var baseSlug = seo.SanitizeSlug(seo.GenerateSlug(title));
            if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "item";

            var candidate = baseSlug;
            var index = 2;

            while (await unique.SlugExistsAsync(candidate, excludeId, ct))
            {
                candidate = $"{baseSlug}-{index++}";
            }
            return candidate;
        }
    }
}