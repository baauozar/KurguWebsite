using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.ValueObjects;
using Microsoft.Extensions.Logging;


namespace KurguWebsite.Application.Services.Concrete
{
    public class CompanyInfoService : ICompanyInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CompanyInfoService> _logger;

        public CompanyInfoService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ICurrentUserService currentUserService,
            ILogger<CompanyInfoService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CompanyInfoDto>> GetCompanyInfoAsync()
        {
            try
            {
                var cachedInfo = await _cacheService.GetAsync<CompanyInfoDto>(CacheKeys.CompanyInfo);

                if (cachedInfo != null)
                    return Result<CompanyInfoDto>.Success(cachedInfo);

                var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();

                if (companyInfo == null)
                {
                    // Create default company info if not exists
                    var contactInfo = ContactInfo.Create(
                        "800-123-4567",
                        "800-123-4568",
                        "info@company.com");

                    var address = Address.Create(
                        "12345 Porto Blvd",
                        "Suite 1500",
                        "Los Angeles",
                        "California",
                        "90000");

                    companyInfo = CompanyInfo.Create("Porto IT Services", contactInfo, address);
                    companyInfo.SetCreatedBy("System");

                    await _unitOfWork.CompanyInfo.AddAsync(companyInfo);
                    await _unitOfWork.CommitAsync();
                }

                var companyInfoDto = _mapper.Map<CompanyInfoDto>(companyInfo);

                await _cacheService.SetAsync(CacheKeys.CompanyInfo, companyInfoDto, TimeSpan.FromMinutes(60));

                return Result<CompanyInfoDto>.Success(companyInfoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company info");
                return Result<CompanyInfoDto>.Failure($"An error occurred while retrieving company information");
            }
        }

        public async Task<Result<CompanyInfoDto>> UpdateBasicInfoAsync(UpdateCompanyInfoDto dto)
        {
            try
            {
                var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();

                if (companyInfo == null)
                    return Result<CompanyInfoDto>.Failure("Company information not found");

                companyInfo.UpdateBasicInfo(
                    dto.CompanyName,
                    dto.About,
                    dto.Mission,
                    dto.Vision,
                    dto.Slogan);

                if (!string.IsNullOrEmpty(dto.LogoPath) || !string.IsNullOrEmpty(dto.LogoLightPath))
                    companyInfo.UpdateLogos(dto.LogoPath, dto.LogoLightPath);

                companyInfo.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.CompanyInfo.UpdateAsync(companyInfo);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearCompanyInfoCacheAsync();

                var companyInfoDto = _mapper.Map<CompanyInfoDto>(companyInfo);
                return Result<CompanyInfoDto>.Success(companyInfoDto, "Company information updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company info");
                return Result<CompanyInfoDto>.Failure($"An error occurred while updating company information");
            }
        }

        public async Task<Result<CompanyInfoDto>> UpdateContactInfoAsync(UpdateContactInfoDto dto)
        {
            try
            {
                var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();

                if (companyInfo == null)
                    return Result<CompanyInfoDto>.Failure("Company information not found");

                var contactInfo = ContactInfo.Create(
                    dto.SupportPhone,
                    dto.SalesPhone,
                    dto.Email);

                companyInfo.UpdateContactInfo(contactInfo);
                companyInfo.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.CompanyInfo.UpdateAsync(companyInfo);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearCompanyInfoCacheAsync();

                var companyInfoDto = _mapper.Map<CompanyInfoDto>(companyInfo);
                return Result<CompanyInfoDto>.Success(companyInfoDto, "Contact information updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact info");
                return Result<CompanyInfoDto>.Failure($"An error occurred while updating contact information");
            }
        }

        public async Task<Result<CompanyInfoDto>> UpdateAddressAsync(UpdateAddressDto dto)
        {
            try
            {
                var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();

                if (companyInfo == null)
                    return Result<CompanyInfoDto>.Failure("Company information not found");

                var address = Address.Create(
                    dto.Street,
                    dto.Suite,
                    dto.City,
                    dto.State,
                    dto.PostalCode,
                    dto.Country);

                companyInfo.UpdateAddress(address);
                companyInfo.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.CompanyInfo.UpdateAsync(companyInfo);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearCompanyInfoCacheAsync();

                var companyInfoDto = _mapper.Map<CompanyInfoDto>(companyInfo);
                return Result<CompanyInfoDto>.Success(companyInfoDto, "Address updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address");
                return Result<CompanyInfoDto>.Failure($"An error occurred while updating address");
            }
        }

        public async Task<Result<CompanyInfoDto>> UpdateSocialMediaAsync(UpdateSocialMediaDto dto)
        {
            try
            {
                var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();

                if (companyInfo == null)
                    return Result<CompanyInfoDto>.Failure("Company information not found");

                var socialMedia = SocialMediaLinks.Create(
                    dto.Facebook,
                    dto.Twitter,
                    dto.LinkedIn,
                    dto.Instagram,
                    dto.YouTube);

                companyInfo.UpdateSocialMedia(socialMedia);
                companyInfo.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.CompanyInfo.UpdateAsync(companyInfo);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearCompanyInfoCacheAsync();

                var companyInfoDto = _mapper.Map<CompanyInfoDto>(companyInfo);
                return Result<CompanyInfoDto>.Success(companyInfoDto, "Social media links updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating social media links");
                return Result<CompanyInfoDto>.Failure($"An error occurred while updating social media links");
            }
        }

        private async Task ClearCompanyInfoCacheAsync()
        {
            await _cacheService.RemoveAsync(CacheKeys.CompanyInfo);
            await _cacheService.RemoveAsync(CacheKeys.HomePage);
            await _cacheService.RemoveAsync(CacheKeys.AboutPage);
            await _cacheService.RemoveAsync(CacheKeys.ContactPage);
        }
    }
}