using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Repositories
{
    public class PageUniquenessChecker : IPageUniquenessChecker
    {
        private readonly KurguWebsiteDbContext _db;
        public PageUniquenessChecker(KurguWebsiteDbContext db) => _db = db;

        public Task<bool> PageSlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
          => _db.Pages.AsNoTracking()
              .AnyAsync(x => x.Slug == slug && (excludeId == null || x.Id != excludeId), ct);
    }
}
