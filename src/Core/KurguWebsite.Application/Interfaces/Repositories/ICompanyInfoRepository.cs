using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Application.Common.Interfaces.Repositories
{
    public interface ICompanyInfoRepository : IGenericRepository<CompanyInfo>
    {
        Task<CompanyInfo?> GetCompanyInfoAsync();
    }
}
