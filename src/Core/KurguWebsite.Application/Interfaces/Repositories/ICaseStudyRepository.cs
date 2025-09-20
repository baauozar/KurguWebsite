using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Application.Common.Interfaces.Repositories
{
    public interface ICaseStudyRepository : IGenericRepository<CaseStudy>
    {
       
        Task<CaseStudy?> GetBySlugAsync(string slug);
        Task<IReadOnlyList<CaseStudy>> GetActiveCaseStudiesAsync();
        Task<IReadOnlyList<CaseStudy>> GetFeaturedCaseStudiesAsync();
        Task<IReadOnlyList<CaseStudy>> GetCaseStudiesByServiceAsync(Guid serviceId);
        Task<IReadOnlyList<CaseStudy>> GetRecentCaseStudiesAsync(int count);
        Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
    }
}
