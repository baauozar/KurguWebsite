using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;

namespace KurguWebsite.Application.Common.Interfaces.Repositories
{
    public interface IPageRepository : IGenericRepository<Page>
    {
        Task<Page?> GetBySlugAsync(string slug);
        Task<Page?> GetByPageTypeAsync(PageType pageType);
        Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
    }
}
