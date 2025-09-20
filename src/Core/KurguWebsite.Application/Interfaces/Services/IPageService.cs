using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Domain.Enums;

namespace KurguWebsite.Application.Common.Interfaces.Services
{
    public interface IPageService
    {
        Task<Result<PageDto>> GetBySlugAsync(string slug);
        Task<Result<PageDto>> GetByTypeAsync(PageType pageType);
        Task<Result<HomePageDto>> GetHomePageDataAsync();
        Task<Result<AboutPageDto>> GetAboutPageDataAsync();
        Task<Result<ContactPageDto>> GetContactPageDataAsync();
        Task<Result<ServicesPageDto>> GetServicesPageDataAsync();

        // Admin Operations
        Task<Result<PageDto>> UpdatePageAsync(Guid id, UpdatePageDto dto);
        Task<Result<bool>> UpdateHeroSectionAsync(Guid id, UpdateHeroSectionDto dto);
    }
}
