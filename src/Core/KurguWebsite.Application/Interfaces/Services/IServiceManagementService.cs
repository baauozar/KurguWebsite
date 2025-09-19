using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;

namespace KurguWebsite.Application.Common.Interfaces.Services
{
    public interface IServiceManagementService
    {
        Task<Result<ServiceDto>> GetByIdAsync(Guid id);
        Task<Result<ServiceDto>> GetBySlugAsync(string slug);
        Task<Result<IEnumerable<ServiceDto>>> GetAllAsync();
        Task<Result<IEnumerable<ServiceDto>>> GetActiveServicesAsync();
        Task<Result<IEnumerable<ServiceDto>>> GetFeaturedServicesAsync();
        Task<Result<ServiceDetailDto>> GetServiceDetailAsync(string slug);
        Task<Result<PaginatedList<ServiceDto>>> GetPaginatedAsync(int pageNumber, int pageSize);

        // Admin Operations
        Task<Result<ServiceDto>> CreateAsync(CreateServiceDto dto);
        Task<Result<ServiceDto>> UpdateAsync(Guid id, UpdateServiceDto dto);
        Task<Result<bool>> DeleteAsync(Guid id);
        Task<Result<bool>> ToggleStatusAsync(Guid id);
        Task<Result<bool>> ToggleFeaturedAsync(Guid id);
        Task<Result<bool>> UpdateDisplayOrderAsync(Guid id, int displayOrder);
    }
}
