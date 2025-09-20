using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Repositories
{
    public class PartnerRepository : GenericRepository<Partner>, IPartnerRepository
    {
        public PartnerRepository(KurguWebsiteDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Partner>> GetActivePartnersAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Partner>> GetPartnersByTypeAsync(PartnerType type)
        {
            return await _dbSet
                .Where(p => p.IsActive && p.Type == type)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }
    }
}