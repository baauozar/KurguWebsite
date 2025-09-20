using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Repositories
{
    public class CompanyInfoRepository : GenericRepository<CompanyInfo>, ICompanyInfoRepository
    {
        public CompanyInfoRepository(KurguWebsiteDbContext context) : base(context)
        {
        }

        public async Task<CompanyInfo?> GetCompanyInfoAsync()
        {
            return await _dbSet.FirstOrDefaultAsync();
        }
    }
}