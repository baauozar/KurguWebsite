using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace KurguWebsite.Application.Services.Concrete
{
    public class CaseStudyService : ICaseStudyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ISeoService _seoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CaseStudyService> _logger;

        public CaseStudyService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ISeoService seoService,
            ICurrentUserService currentUserService,
            ILogger<CaseStudyService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _seoService = seoService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CaseStudyDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(id);

                if (caseStudy == null)
                    return Result<CaseStudyDto>.Failure($"Case study with id {id} not found");

                var caseStudyDto = _mapper.Map<CaseStudyDto>(caseStudy);
                return Result<CaseStudyDto>.Success(caseStudyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving case study by id {CaseStudyId}", id);
                return Result<CaseStudyDto>.Failure($"An error occurred while retrieving the case study");
            }
        }

        public async Task<Result<CaseStudyDto>> GetBySlugAsync(string slug)
        {
            try
            {
                var caseStudy = await _unitOfWork.CaseStudies.GetBySlugAsync(slug);

                if (caseStudy == null)
                    return Result<CaseStudyDto>.Failure($"Case study with slug '{slug}' not found");

                var caseStudyDto = _mapper.Map<CaseStudyDto>(caseStudy);
                return Result<CaseStudyDto>.Success(caseStudyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving case study by slug {Slug}", slug);
                return Result<CaseStudyDto>.Failure($"An error occurred while retrieving the case study");
            }
        }

        public async Task<Result<IEnumerable<CaseStudyDto>>> GetAllAsync()
        {
            try
            {
                var caseStudies = await _unitOfWork.CaseStudies.GetAllAsync();
                var caseStudyDtos = _mapper.Map<IEnumerable<CaseStudyDto>>(caseStudies);

                return Result<IEnumerable<CaseStudyDto>>.Success(caseStudyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all case studies");
                return Result<IEnumerable<CaseStudyDto>>.Failure($"An error occurred while retrieving case studies");
            }
        }

        public async Task<Result<IEnumerable<CaseStudyDto>>> GetFeaturedAsync()
        {
            try
            {
                var cachedCaseStudies = await _cacheService.GetAsync<IEnumerable<CaseStudyDto>>(CacheKeys.FeaturedCaseStudies);

                if (cachedCaseStudies != null)
                    return Result<IEnumerable<CaseStudyDto>>.Success(cachedCaseStudies);

                var caseStudies = await _unitOfWork.CaseStudies.GetFeaturedCaseStudiesAsync();
                var caseStudyDtos = _mapper.Map<IEnumerable<CaseStudyDto>>(caseStudies);

                await _cacheService.SetAsync(CacheKeys.FeaturedCaseStudies, caseStudyDtos, TimeSpan.FromMinutes(30));

                return Result<IEnumerable<CaseStudyDto>>.Success(caseStudyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured case studies");
                return Result<IEnumerable<CaseStudyDto>>.Failure($"An error occurred while retrieving featured case studies");
            }
        }

        public async Task<Result<IEnumerable<CaseStudyDto>>> GetByServiceAsync(Guid serviceId)
        {
            try
            {
                var caseStudies = await _unitOfWork.CaseStudies.GetCaseStudiesByServiceAsync(serviceId);
                var caseStudyDtos = _mapper.Map<IEnumerable<CaseStudyDto>>(caseStudies);

                return Result<IEnumerable<CaseStudyDto>>.Success(caseStudyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving case studies for service {ServiceId}", serviceId);
                return Result<IEnumerable<CaseStudyDto>>.Failure($"An error occurred while retrieving case studies");
            }
        }

        public async Task<Result<PaginatedList<CaseStudyDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var caseStudies = await _unitOfWork.CaseStudies.GetAsync(
    orderBy: q => q.OrderByDescending(cs => cs.CompletedDate),
    includeString: null,
    disableTracking: true);
                var totalCount = caseStudies.Count;
                var items = caseStudies
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var caseStudyDtos = _mapper.Map<List<CaseStudyDto>>(items);
                var paginatedList = new PaginatedList<CaseStudyDto>(caseStudyDtos, totalCount, pageNumber, pageSize);

                return Result<PaginatedList<CaseStudyDto>>.Success(paginatedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated case studies");
                return Result<PaginatedList<CaseStudyDto>>.Failure($"An error occurred while retrieving case studies");
            }
        }

        public async Task<Result<CaseStudyDto>> CreateAsync(CreateCaseStudyDto dto)
        {
            try
            {
                // Check if slug already exists
                var slug = _seoService.GenerateSlug(dto.Title);
                if (await _unitOfWork.CaseStudies.SlugExistsAsync(slug))
                    return Result<CaseStudyDto>.Failure($"A case study with a similar title already exists");

                var caseStudy = CaseStudy.Create(
                    dto.Title,
                    dto.ClientName,
                    dto.Description,
                    dto.ImagePath,
                    dto.CompletedDate);

                caseStudy.Update(
                    dto.Title,
                    dto.ClientName,
                    dto.Description,
                    dto.Challenge,
                    dto.Solution,
                    dto.Result);

                if (dto.ServiceId.HasValue)
                    caseStudy.SetService(dto.ServiceId.Value);

                if (dto.Technologies != null && dto.Technologies.Any())
                {
                    foreach (var tech in dto.Technologies)
                    {
                        caseStudy.AddTechnology(tech);
                    }
                }

                caseStudy.SetFeatured(dto.IsFeatured);
                caseStudy.SetCreatedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.CaseStudies.AddAsync(caseStudy);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearCaseStudyCacheAsync();

                var caseStudyDto = _mapper.Map<CaseStudyDto>(caseStudy);
                return Result<CaseStudyDto>.Success(caseStudyDto, "Case study created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating case study");
                return Result<CaseStudyDto>.Failure($"An error occurred while creating the case study");
            }
        }

        public async Task<Result<CaseStudyDto>> UpdateAsync(Guid id, UpdateCaseStudyDto dto)
        {
            try
            {
                var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(id);

                if (caseStudy == null)
                    return Result<CaseStudyDto>.Failure($"Case study with id {id} not found");

                // Check if new slug conflicts
                var newSlug = _seoService.GenerateSlug(dto.Title);
                if (await _unitOfWork.CaseStudies.SlugExistsAsync(newSlug, id))
                    return Result<CaseStudyDto>.Failure($"A case study with a similar title already exists");

                caseStudy.Update(
                    dto.Title,
                    dto.ClientName,
                    dto.Description,
                    dto.Challenge,
                    dto.Solution,
                    dto.Result);

                // In UpdateAsync method, replace this line:


                // With this null check:
                if (dto.ServiceId.HasValue)
                    caseStudy.SetService(dto.ServiceId.Value);

                if (!dto.IsActive)
                    caseStudy.SoftDelete(_currentUserService.UserId ?? "System");
                else
                    caseStudy.Restore();

                caseStudy.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.CaseStudies.UpdateAsync(caseStudy);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearCaseStudyCacheAsync();

                var caseStudyDto = _mapper.Map<CaseStudyDto>(caseStudy);
                return Result<CaseStudyDto>.Success(caseStudyDto, "Case study updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating case study {CaseStudyId}", id);
                return Result<CaseStudyDto>.Failure($"An error occurred while updating the case study");
            }
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(id);

                if (caseStudy == null)
                    return Result<bool>.Failure($"Case study with id {id} not found");

                caseStudy.SoftDelete(_currentUserService.UserId ?? "System");

                await _unitOfWork.CaseStudies.UpdateAsync(caseStudy);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearCaseStudyCacheAsync();

                return Result<bool>.Success(true, "Case study deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting case study {CaseStudyId}", id);
                return Result<bool>.Failure($"An error occurred while deleting the case study");
            }
        }

        public async Task<Result<bool>> ToggleFeaturedAsync(Guid id)
        {
            try
            {
                var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(id);

                if (caseStudy == null)
                    return Result<bool>.Failure($"Case study with id {id} not found");

                caseStudy.SetFeatured(!caseStudy.IsFeatured);
                caseStudy.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.CaseStudies.UpdateAsync(caseStudy);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearCaseStudyCacheAsync();

                return Result<bool>.Success(true, $"Case study {(caseStudy.IsFeatured ? "featured" : "unfeatured")} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling case study featured status {CaseStudyId}", id);
                return Result<bool>.Failure($"An error occurred while updating the case study");
            }
        }

        private async Task ClearCaseStudyCacheAsync()
        {
            await _cacheService.RemoveAsync(CacheKeys.CaseStudies);
            await _cacheService.RemoveAsync(CacheKeys.FeaturedCaseStudies);
            await _cacheService.RemoveAsync(CacheKeys.HomePage);
        }
    }
}