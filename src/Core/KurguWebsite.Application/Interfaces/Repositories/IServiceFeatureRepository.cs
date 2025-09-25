using KurguWebsite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Interfaces.Repositories
{
    public interface IServiceFeatureRepository : IGenericRepository<ServiceFeature>
    {
        Task<List<ServiceFeature>> GetByServiceIdAsync(Guid serviceId);
    }
}
