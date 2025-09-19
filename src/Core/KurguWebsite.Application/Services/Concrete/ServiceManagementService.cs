using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Services.Concrete
{
    public class ServiceManagementService : IServiceManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ISeoService _seoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ServiceManagementService> _logger;

        public ServiceManagementService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ISeoService seoService,
            ICurrentUserService currentUserService,
            ILogger<ServiceManagementService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _seoService = seoService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ServiceDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var cacheKey = string.Format(CacheKeys.ServiceById, id);
                var cachedService = await _cacheService.GetAsync<ServiceDto>(cacheKey);

                if (cachedService != null)
                    return Result<ServiceDto>.Success(cachedService);

                var service = await _unitOfWork.Services.GetByIdAsync(id);

                if (service == null)
                    return Result<ServiceDto>.Failure($"Service with id {id} not found");

                var serviceDto = _mapper.Map<ServiceDto>(service);

                await _cacheService.SetAsync(cacheKey, serviceDto, TimeSpan.FromMinutes(30));

                return Result<ServiceDto>.Success(serviceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service by id {ServiceId}", id);
                return Result<ServiceDto>.Failure($"An error occurred while retrieving the service");
            }
        }

        public async Task<Result<ServiceDto>> GetBySlugAsync(string slug)
        {
            try
            {
                var cacheKey = string.Format(CacheKeys.ServiceBySlug, slug);
                var cachedService = await _cacheService.GetAsync<ServiceDto>(cacheKey);

                if (cachedService != null)
                    return Result<ServiceDto>.Success(cachedService);

                var service = await _unitOfWork.Services.GetBySlugAsync(slug);

                if (service == null)
                    return Result<ServiceDto>.Failure($"Service with slug '{slug}' not found");

                var serviceDto = _mapper.Map<ServiceDto>(service);

                await _cacheService.SetAsync(cacheKey, serviceDto, TimeSpan.FromMinutes(30));

                return Result<ServiceDto>.Success(serviceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service by slug {Slug}", slug);
                return Result<ServiceDto>.Failure($"An error occurred while retrieving the service");
            }
        }

        public async Task<Result<IEnumerable<ServiceDto>>> GetAllAsync()
        {
            try
            {
                var services = await _unitOfWork.Services.GetAllAsync();
                var serviceDtos = _mapper.Map<IEnumerable<ServiceDto>>(services);

                return Result<IEnumerable<ServiceDto>>.Success(serviceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all services");
                return Result<IEnumerable<ServiceDto>>.Failure($"An error occurred while retrieving services");
            }
        }

        public async Task<Result<IEnumerable<ServiceDto>>> GetActiveServicesAsync()
        {
            try
            {
                var cachedServices = await _cacheService.GetAsync<IEnumerable<ServiceDto>>(CacheKeys.ActiveServices);

                if (cachedServices != null)
                    return Result<IEnumerable<ServiceDto>>.Success(cachedServices);

                var services = await _unitOfWork.Services.GetActiveServicesAsync();
                var serviceDtos = _mapper.Map<IEnumerable<ServiceDto>>(services);

                await _cacheService.SetAsync(CacheKeys.ActiveServices, serviceDtos, TimeSpan.FromMinutes(30));

                return Result<IEnumerable<ServiceDto>>.Success(serviceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active services");
                return Result<IEnumerable<ServiceDto>>.Failure($"An error occurred while retrieving active services");
            }
        }

        public async Task<Result<IEnumerable<ServiceDto>>> GetFeaturedServicesAsync()
        {
            try
            {
                var cachedServices = await _cacheService.GetAsync<IEnumerable<ServiceDto>>(CacheKeys.FeaturedServices);

                if (cachedServices != null)
                    return Result<IEnumerable<ServiceDto>>.Success(cachedServices);

                var services = await _unitOfWork.Services.GetFeaturedServicesAsync();
                var serviceDtos = _mapper.Map<IEnumerable<ServiceDto>>(services);

                await _cacheService.SetAsync(CacheKeys.FeaturedServices, serviceDtos, TimeSpan.FromMinutes(30));

                return Result<IEnumerable<ServiceDto>>.Success(serviceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured services");
                return Result<IEnumerable<ServiceDto>>.Failure($"An error occurred while retrieving featured services");
            }
        }

        public async Task<Result<ServiceDetailDto>> GetServiceDetailAsync(string slug)
        {
            try
            {
                var service = await _unitOfWork.Services.GetServiceWithFeaturesAsync(
                    (await _unitOfWork.Services.GetBySlugAsync(slug))?.Id ?? Guid.Empty);

                if (service == null)
                    return Result<ServiceDetailDto>.Failure($"Service with slug '{slug}' not found");

                var serviceDetail = _mapper.Map<ServiceDetailDto>(service);

                // Get related case studies
                var caseStudies = await _unitOfWork.CaseStudies.GetCaseStudiesByServiceAsync(service.Id);
                serviceDetail.RelatedCaseStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies);

                // Get other services
                var otherServices = await _unitOfWork.Services.GetActiveServicesAsync();
                serviceDetail.OtherServices = _mapper.Map<List<ServiceDto>>(
                    otherServices.Where(s => s.Id != service.Id).Take(5));

                return Result<ServiceDetailDto>.Success(serviceDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service detail for slug {Slug}", slug);
                return Result<ServiceDetailDto>.Failure($"An error occurred while retrieving service details");
            }
        }

        public async Task<Result<PaginatedList<ServiceDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                // Specify null for includes and use the overload with string? includeString = null
                var services = await _unitOfWork.Services.GetAsync(
                    predicate: null,
                    orderBy: q => q.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Title),
                    includeString: null,
                    disableTracking: true
                );

                var totalCount = services.Count;
                var items = services
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var serviceDtos = _mapper.Map<List<ServiceDto>>(items);
                var paginatedList = new PaginatedList<ServiceDto>(serviceDtos, totalCount, pageNumber, pageSize);

                return Result<PaginatedList<ServiceDto>>.Success(paginatedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated services");
                return Result<PaginatedList<ServiceDto>>.Failure($"An error occurred while retrieving services");
            }
        }

        public async Task<Result<ServiceDto>> CreateAsync(CreateServiceDto dto)
        {
            try
            {
                // Check if slug already exists
                var slug = _seoService.GenerateSlug(dto.Title);
                if (await _unitOfWork.Services.SlugExistsAsync(slug))
                    return Result<ServiceDto>.Failure($"A service with a similar title already exists");

                var service = Service.Create(
                    dto.Title,
                    dto.Description,
                    dto.ShortDescription,
                    dto.IconPath,
                    dto.Category);

                service.SetDisplayOrder(dto.DisplayOrder);
                service.SetFeatured(dto.IsFeatured);

                if (!string.IsNullOrWhiteSpace(dto.FullDescription))
                    service.Update(dto.Title, dto.Description, dto.ShortDescription,
                        dto.FullDescription, dto.IconPath, dto.Category);

                service.UpdateSeo(dto.MetaTitle, dto.MetaDescription, dto.MetaKeywords);
                service.SetCreatedBy(_currentUserService.UserId ?? "System");

                // Add features if any
                if (dto.Features != null && dto.Features.Any())
                {
                    foreach (var featureDto in dto.Features)
                    {
                        var feature = ServiceFeature.Create(
                            service.Id,
                            featureDto.Title,
                            featureDto.Description,
                            featureDto.IconClass);
                        service.AddFeature(feature);
                    }
                }

                await _unitOfWork.Services.AddAsync(service);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearServiceCacheAsync();

                var serviceDto = _mapper.Map<ServiceDto>(service);
                return Result<ServiceDto>.Success(serviceDto, "Service created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                return Result<ServiceDto>.Failure($"An error occurred while creating the service");
            }
        }

        public async Task<Result<ServiceDto>> UpdateAsync(Guid id, UpdateServiceDto dto)
        {
            try
            {
                var service = await _unitOfWork.Services.GetByIdAsync(id);

                if (service == null)
                    return Result<ServiceDto>.Failure($"Service with id {id} not found");

                // Check if new slug conflicts
                var newSlug = _seoService.GenerateSlug(dto.Title);
                if (await _unitOfWork.Services.SlugExistsAsync(newSlug, id))
                    return Result<ServiceDto>.Failure($"A service with a similar title already exists");

                service.Update(
                    dto.Title,
                    dto.Description,
                    dto.ShortDescription,
                    dto.FullDescription,
                    dto.IconPath,
                    dto.Category);

                service.SetDisplayOrder(dto.DisplayOrder);
                service.SetFeatured(dto.IsFeatured);
                service.UpdateSeo(dto.MetaTitle, dto.MetaDescription, dto.MetaKeywords);

                if (dto.IsActive)
                    service.Activate();
                else
                    service.Deactivate();

                service.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Services.UpdateAsync(service);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearServiceCacheAsync();

                var serviceDto = _mapper.Map<ServiceDto>(service);
                return Result<ServiceDto>.Success(serviceDto, "Service updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service {ServiceId}", id);
                return Result<ServiceDto>.Failure($"An error occurred while updating the service");
            }
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var service = await _unitOfWork.Services.GetByIdAsync(id);

                if (service == null)
                    return Result<bool>.Failure($"Service with id {id} not found");

                service.SoftDelete(_currentUserService.UserId ?? "System");

                await _unitOfWork.Services.UpdateAsync(service);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearServiceCacheAsync();

                return Result<bool>.Success(true, "Service deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {ServiceId}", id);
                return Result<bool>.Failure($"An error occurred while deleting the service");
            }
        }

        public async Task<Result<bool>> ToggleStatusAsync(Guid id)
        {
            try
            {
                var service = await _unitOfWork.Services.GetByIdAsync(id);

                if (service == null)
                    return Result<bool>.Failure($"Service with id {id} not found");

                if (service.IsActive)
                    service.Deactivate();
                else
                    service.Activate();

                service.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Services.UpdateAsync(service);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearServiceCacheAsync();

                return Result<bool>.Success(true, $"Service {(service.IsActive ? "activated" : "deactivated")} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling service status {ServiceId}", id);
                return Result<bool>.Failure($"An error occurred while updating the service status");
            }
        }

        public async Task<Result<bool>> ToggleFeaturedAsync(Guid id)
        {
            try
            {
                var service = await _unitOfWork.Services.GetByIdAsync(id);

                if (service == null)
                    return Result<bool>.Failure($"Service with id {id} not found");

                service.SetFeatured(!service.IsFeatured);
                service.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Services.UpdateAsync(service);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearServiceCacheAsync();

                return Result<bool>.Success(true, $"Service {(service.IsFeatured ? "featured" : "unfeatured")} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling service featured status {ServiceId}", id);
                return Result<bool>.Failure($"An error occurred while updating the service");
            }
        }

        public async Task<Result<bool>> UpdateDisplayOrderAsync(Guid id, int displayOrder)
        {
            try
            {
                var service = await _unitOfWork.Services.GetByIdAsync(id);

                if (service == null)
                    return Result<bool>.Failure($"Service with id {id} not found");

                service.SetDisplayOrder(displayOrder);
                service.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Services.UpdateAsync(service);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearServiceCacheAsync();

                return Result<bool>.Success(true, "Display order updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service display order {ServiceId}", id);
                return Result<bool>.Failure($"An error occurred while updating the display order");
            }
        }

        private async Task ClearServiceCacheAsync()
        {
            await _cacheService.RemoveByPrefixAsync("service");
            await _cacheService.RemoveAsync(CacheKeys.Services);
            await _cacheService.RemoveAsync(CacheKeys.FeaturedServices);
            await _cacheService.RemoveAsync(CacheKeys.ActiveServices);
            await _cacheService.RemoveAsync(CacheKeys.HomePage);
            await _cacheService.RemoveAsync(CacheKeys.ServicesPage);
        }
    }
}