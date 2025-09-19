using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;

namespace KurguWebsite.Application.Common.Interfaces.Services
{
    public interface ICompanyInfoService
    {
        Task<Result<CompanyInfoDto>> GetCompanyInfoAsync();
        Task<Result<CompanyInfoDto>> UpdateBasicInfoAsync(UpdateCompanyInfoDto dto);
        Task<Result<CompanyInfoDto>> UpdateContactInfoAsync(UpdateContactInfoDto dto);
        Task<Result<CompanyInfoDto>> UpdateAddressAsync(UpdateAddressDto dto);
        Task<Result<CompanyInfoDto>> UpdateSocialMediaAsync(UpdateSocialMediaDto dto);
    }
}
