using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;

namespace KurguWebsite.Application.Common.Interfaces.Services
{
    public interface ICaseStudyService
    {
        Task<Result<CaseStudyDto>> GetByIdAsync(Guid id);
        Task<Result<CaseStudyDto>> GetBySlugAsync(string slug);
        Task<Result<IEnumerable<CaseStudyDto>>> GetAllAsync();
        Task<Result<IEnumerable<CaseStudyDto>>> GetFeaturedAsync();
        Task<Result<IEnumerable<CaseStudyDto>>> GetByServiceAsync(Guid serviceId);
        Task<Result<PaginatedList<CaseStudyDto>>> GetPaginatedAsync(int pageNumber, int pageSize);

        // Admin Operations
        Task<Result<CaseStudyDto>> CreateAsync(CreateCaseStudyDto dto);
        Task<Result<CaseStudyDto>> UpdateAsync(Guid id, UpdateCaseStudyDto dto);
        Task<Result<bool>> DeleteAsync(Guid id);
        Task<Result<bool>> ToggleFeaturedAsync(Guid id);
    }
}
