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
    public class ServiceUniquenessChecker : IServiceUniquenessChecker
    {
        private readonly KurguWebsiteDbContext _db;

        public ServiceUniquenessChecker(KurguWebsiteDbContext db) => _db = db;

        public Task<bool> TitleExistsAsync(string title, Guid? excludeId = null, CancellationToken ct = default)
            => _db.Services
                  .AsNoTracking()
                  .AnyAsync(s => s.Title == title && (excludeId == null || s.Id != excludeId), ct);

        public Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
            => _db.Services
                  .AsNoTracking()
                  .AnyAsync(s => s.Slug == slug && (excludeId == null || s.Id != excludeId), ct);
    }
}