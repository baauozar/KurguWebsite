using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Services.Concrete
{
    public class TestimonialService : ITestimonialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<TestimonialService> _logger;

        public TestimonialService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ICurrentUserService currentUserService,
            ILogger<TestimonialService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<TestimonialDto>>> GetAllAsync()
        {
            try
            {
                var testimonials = await _unitOfWork.Testimonials.GetAllAsync();
                var testimonialDtos = _mapper.Map<IEnumerable<TestimonialDto>>(testimonials);

                return Result<IEnumerable<TestimonialDto>>.Success(testimonialDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all testimonials");
                return Result<IEnumerable<TestimonialDto>>.Failure($"An error occurred while retrieving testimonials");
            }
        }

        public async Task<Result<IEnumerable<TestimonialDto>>> GetActiveAsync()
        {
            try
            {
                var cachedTestimonials = await _cacheService.GetAsync<IEnumerable<TestimonialDto>>(CacheKeys.ActiveTestimonials);

                if (cachedTestimonials != null)
                    return Result<IEnumerable<TestimonialDto>>.Success(cachedTestimonials);

                var testimonials = await _unitOfWork.Testimonials.GetActiveTestimonialsAsync();
                var testimonialDtos = _mapper.Map<IEnumerable<TestimonialDto>>(testimonials);

                await _cacheService.SetAsync(CacheKeys.ActiveTestimonials, testimonialDtos, TimeSpan.FromMinutes(30));

                return Result<IEnumerable<TestimonialDto>>.Success(testimonialDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active testimonials");
                return Result<IEnumerable<TestimonialDto>>.Failure($"An error occurred while retrieving testimonials");
            }
        }

        public async Task<Result<TestimonialDto>> GetRandomAsync()
        {
            try
            {
                var testimonial = await _unitOfWork.Testimonials.GetRandomTestimonialAsync();

                if (testimonial == null)
                    return Result<TestimonialDto>.Failure("No testimonials available");

                var testimonialDto = _mapper.Map<TestimonialDto>(testimonial);
                return Result<TestimonialDto>.Success(testimonialDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving random testimonial");
                return Result<TestimonialDto>.Failure($"An error occurred while retrieving a testimonial");
            }
        }

        public async Task<Result<PaginatedList<TestimonialDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                // Specify null for the ambiguous parameter to resolve the overload
                var testimonials = await _unitOfWork.Testimonials.GetAsync(
                    predicate: null,
                    orderBy: q => q.OrderBy(t => t.DisplayOrder).ThenByDescending(t => t.TestimonialDate),
                    includeString: null,
                    disableTracking: true);

                var totalCount = testimonials.Count;
                var items = testimonials
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var testimonialDtos = _mapper.Map<List<TestimonialDto>>(items);
                var paginatedList = new PaginatedList<TestimonialDto>(testimonialDtos, totalCount, pageNumber, pageSize);

                return Result<PaginatedList<TestimonialDto>>.Success(paginatedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated testimonials");
                return Result<PaginatedList<TestimonialDto>>.Failure($"An error occurred while retrieving testimonials");
            }
        }

        public async Task<Result<TestimonialDto>> CreateAsync(CreateTestimonialDto dto)
        {
            try
            {
                var testimonial = Testimonial.Create(
                    dto.ClientName,
                    dto.ClientTitle,
                    dto.CompanyName,
                    dto.Content,
                    dto.Rating);

                testimonial.SetFeatured(dto.IsFeatured);
                testimonial.SetCreatedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Testimonials.AddAsync(testimonial);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearTestimonialCacheAsync();

                var testimonialDto = _mapper.Map<TestimonialDto>(testimonial);
                return Result<TestimonialDto>.Success(testimonialDto, "Testimonial created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating testimonial");
                return Result<TestimonialDto>.Failure($"An error occurred while creating the testimonial");
            }
        }

        public async Task<Result<TestimonialDto>> UpdateAsync(Guid id, UpdateTestimonialDto dto)
        {
            try
            {
                var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(id);

                if (testimonial == null)
                    return Result<TestimonialDto>.Failure($"Testimonial with id {id} not found");

                testimonial.Update(
                    dto.ClientName,
                    dto.ClientTitle,
                    dto.CompanyName,
                    dto.Content,
                    dto.Rating);

                testimonial.SetFeatured(dto.IsFeatured);

                if (dto.IsActive)
                    testimonial.Activate();
                else
                    testimonial.Deactivate();

                testimonial.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Testimonials.UpdateAsync(testimonial);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearTestimonialCacheAsync();

                var testimonialDto = _mapper.Map<TestimonialDto>(testimonial);
                return Result<TestimonialDto>.Success(testimonialDto, "Testimonial updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating testimonial {TestimonialId}", id);
                return Result<TestimonialDto>.Failure($"An error occurred while updating the testimonial");
            }
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(id);

                if (testimonial == null)
                    return Result<bool>.Failure($"Testimonial with id {id} not found");

                testimonial.SoftDelete(_currentUserService.UserId ?? "System");

                await _unitOfWork.Testimonials.UpdateAsync(testimonial);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearTestimonialCacheAsync();

                return Result<bool>.Success(true, "Testimonial deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting testimonial {TestimonialId}", id);
                return Result<bool>.Failure($"An error occurred while deleting the testimonial");
            }
        }

        public async Task<Result<bool>> ToggleStatusAsync(Guid id)
        {
            try
            {
                var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(id);

                if (testimonial == null)
                    return Result<bool>.Failure($"Testimonial with id {id} not found");

                if (testimonial.IsActive)
                    testimonial.Deactivate();
                else
                    testimonial.Activate();

                testimonial.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Testimonials.UpdateAsync(testimonial);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearTestimonialCacheAsync();

                return Result<bool>.Success(true, $"Testimonial {(testimonial.IsActive ? "activated" : "deactivated")} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling testimonial status {TestimonialId}", id);
                return Result<bool>.Failure($"An error occurred while updating the testimonial status");
            }
        }

        private async Task ClearTestimonialCacheAsync()
        {
            await _cacheService.RemoveAsync(CacheKeys.Testimonials);
            await _cacheService.RemoveAsync(CacheKeys.ActiveTestimonials);
            await _cacheService.RemoveAsync(CacheKeys.HomePage);
            await _cacheService.RemoveAsync(CacheKeys.AboutPage);
        }
    }
}