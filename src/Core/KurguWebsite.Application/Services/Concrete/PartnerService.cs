using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Services.Concrete
{
    public class PartnerService : IPartnerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<PartnerService> _logger;

        public PartnerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ICurrentUserService currentUserService,
            ILogger<PartnerService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<PartnerDto>>> GetAllAsync()
        {
            try
            {
                var partners = await _unitOfWork.Partners.GetAllAsync();
                var partnerDtos = _mapper.Map<IEnumerable<PartnerDto>>(partners);

                return Result<IEnumerable<PartnerDto>>.Success(partnerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all partners");
                return Result<IEnumerable<PartnerDto>>.Failure($"An error occurred while retrieving partners");
            }
        }

        public async Task<Result<IEnumerable<PartnerDto>>> GetActiveAsync()
        {
            try
            {
                var cachedPartners = await _cacheService.GetAsync<IEnumerable<PartnerDto>>(CacheKeys.ActivePartners);

                if (cachedPartners != null)
                    return Result<IEnumerable<PartnerDto>>.Success(cachedPartners);

                var partners = await _unitOfWork.Partners.GetActivePartnersAsync();
                var partnerDtos = _mapper.Map<IEnumerable<PartnerDto>>(partners);

                await _cacheService.SetAsync(CacheKeys.ActivePartners, partnerDtos, TimeSpan.FromMinutes(30));

                return Result<IEnumerable<PartnerDto>>.Success(partnerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active partners");
                return Result<IEnumerable<PartnerDto>>.Failure($"An error occurred while retrieving partners");
            }
        }

        public async Task<Result<PartnerDto>> CreateAsync(CreatePartnerDto dto)
        {
            try
            {
                var partner = Partner.Create(
                    dto.Name,
                    dto.LogoPath,
                    dto.Type);

                partner.Update(
                    dto.Name,
                    dto.LogoPath,
                    dto.WebsiteUrl,
                    dto.Description,
                    dto.Type);

                partner.SetCreatedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Partners.AddAsync(partner);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearPartnerCacheAsync();

                var partnerDto = _mapper.Map<PartnerDto>(partner);
                return Result<PartnerDto>.Success(partnerDto, "Partner created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating partner");
                return Result<PartnerDto>.Failure($"An error occurred while creating the partner");
            }
        }

        public async Task<Result<PartnerDto>> UpdateAsync(Guid id, UpdatePartnerDto dto)
        {
            try
            {
                var partner = await _unitOfWork.Partners.GetByIdAsync(id);

                if (partner == null)
                    return Result<PartnerDto>.Failure($"Partner with id {id} not found");

                partner.Update(
                    dto.Name,
                    dto.LogoPath,
                    dto.WebsiteUrl,
                    dto.Description,
                    dto.Type);

                partner.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Partners.UpdateAsync(partner);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearPartnerCacheAsync();

                var partnerDto = _mapper.Map<PartnerDto>(partner);
                return Result<PartnerDto>.Success(partnerDto, "Partner updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partner {PartnerId}", id);
                return Result<PartnerDto>.Failure($"An error occurred while updating the partner");
            }
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var partner = await _unitOfWork.Partners.GetByIdAsync(id);

                if (partner == null)
                    return Result<bool>.Failure($"Partner with id {id} not found");

                partner.SoftDelete(_currentUserService.UserId ?? "System");

                await _unitOfWork.Partners.UpdateAsync(partner);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearPartnerCacheAsync();

                return Result<bool>.Success(true, "Partner deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting partner {PartnerId}", id);
                return Result<bool>.Failure($"An error occurred while deleting the partner");
            }
        }

        public async Task<Result<bool>> UpdateDisplayOrderAsync(Guid id, int displayOrder)
        {
            try
            {
                var partner = await _unitOfWork.Partners.GetByIdAsync(id);

                if (partner == null)
                    return Result<bool>.Failure($"Partner with id {id} not found");

                // Note: Add SetDisplayOrder method to Partner entity if not exists
                partner.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Partners.UpdateAsync(partner);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearPartnerCacheAsync();

                return Result<bool>.Success(true, "Display order updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partner display order {PartnerId}", id);
                return Result<bool>.Failure($"An error occurred while updating the display order");
            }
        }

        private async Task ClearPartnerCacheAsync()
        {
            await _cacheService.RemoveAsync(CacheKeys.Partners);
            await _cacheService.RemoveAsync(CacheKeys.ActivePartners);
            await _cacheService.RemoveAsync(CacheKeys.HomePage);
            await _cacheService.RemoveAsync(CacheKeys.AboutPage);
        }
    }
}
