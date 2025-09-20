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
    public class ProcessStepRepository : GenericRepository<ProcessStep>, IProcessStepRepository
    {
        public ProcessStepRepository(KurguWebsiteDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<ProcessStep>> GetActiveStepsOrderedAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.StepNumber)
                .ToListAsync();
        }
    }
}