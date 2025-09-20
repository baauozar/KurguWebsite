using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;

namespace KurguWebsite.Application.Common.Interfaces.Services
{
    public interface ITestimonialService
    {
        Task<Result<IEnumerable<TestimonialDto>>> GetAllAsync();
        Task<Result<IEnumerable<TestimonialDto>>> GetActiveAsync();
        Task<Result<TestimonialDto>> GetRandomAsync();
        Task<Result<PaginatedList<TestimonialDto>>> GetPaginatedAsync(int pageNumber, int pageSize);

        // Admin Operations
        Task<Result<TestimonialDto>> CreateAsync(CreateTestimonialDto dto);
        Task<Result<TestimonialDto>> UpdateAsync(Guid id, UpdateTestimonialDto dto);
        Task<Result<bool>> DeleteAsync(Guid id);
        Task<Result<bool>> ToggleStatusAsync(Guid id);
    }
}
