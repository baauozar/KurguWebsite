using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;

namespace KurguWebsite.Application.Common.Interfaces.Services
{
    public interface IPartnerService
    {
        Task<Result<IEnumerable<PartnerDto>>> GetAllAsync();
        Task<Result<IEnumerable<PartnerDto>>> GetActiveAsync();
        Task<Result<PartnerDto>> CreateAsync(CreatePartnerDto dto);
        Task<Result<PartnerDto>> UpdateAsync(Guid id, UpdatePartnerDto dto);
        Task<Result<bool>> DeleteAsync(Guid id);
        Task<Result<bool>> UpdateDisplayOrderAsync(Guid id, int displayOrder);
    }
}
