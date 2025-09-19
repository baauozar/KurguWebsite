using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;

namespace KurguWebsite.Application.Common.Interfaces.Repositories
{
    public interface IPartnerRepository : IGenericRepository<Partner>
    {
        Task<IReadOnlyList<Partner>> GetActivePartnersAsync();
        Task<IReadOnlyList<Partner>> GetPartnersByTypeAsync(PartnerType type);
    }
}
