// src/Core/KurguWebsite.Application/Features/Page/Queries/GetAboutPageDataQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Enums;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class GetAboutPageDataQuery : IRequest<Result<AboutPageDto>> { }

    public class GetAboutPageDataQueryHandler : IRequestHandler<GetAboutPageDataQuery, Result<AboutPageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetAboutPageDataQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<AboutPageDto>> Handle(
            GetAboutPageDataQuery request,
            CancellationToken cancellationToken)
        {
            // Check cache first
            var cachedPage = await _cacheService.GetAsync<AboutPageDto>(CacheKeys.AboutPage);
            if (cachedPage != null)
            {
                return Result<AboutPageDto>.Success(cachedPage);
            }

            // Get About page content
            var page = await _unitOfWork.Pages.GetByPageTypeAsync(PageType.About);
            if (page == null)
                return Result<AboutPageDto>.Failure("About page content not found.");

            var aboutPageDto = new AboutPageDto
            {
                PageInfo = _mapper.Map<PageDto>(page)
            };

            // Get company info (includes statistics, section images, etc.)
            var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
            if (companyInfo != null)
            {
                aboutPageDto.CompanyInfo = _mapper.Map<CompanyInfoDto>(companyInfo);
            }

            // Get featured testimonials for the testimonial section
            var testimonials = await _unitOfWork.Testimonials.GetFeaturedTestimonialsAsync();
            aboutPageDto.Testimonials = _mapper.Map<List<TestimonialDto>>(testimonials.Take(1)); // Only 1 testimonial shown

            // Get all active partners (client logos)
            var partners = await _unitOfWork.Partners.GetActivePartnersAsync();
            aboutPageDto.Partners = _mapper.Map<List<PartnerDto>>(partners.Take(8)); // Show 8 client logos

            // Cache the result
            await _cacheService.SetAsync(
                CacheKeys.AboutPage,
                aboutPageDto,
                TimeSpan.FromMinutes(30));

            return Result<AboutPageDto>.Success(aboutPageDto);
        }
    }
}