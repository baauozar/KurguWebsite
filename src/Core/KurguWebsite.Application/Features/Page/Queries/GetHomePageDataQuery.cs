using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
// Add other DTOs needed for the home page
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Domain.Enums;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class GetHomePageDataQuery : IRequest<Result<HomePageDto>> { }

    public class GetHomePageDataQueryHandler : IRequestHandler<GetHomePageDataQuery, Result<HomePageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetHomePageDataQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<HomePageDto>> Handle(GetHomePageDataQuery request, CancellationToken cancellationToken)
        {
            var cachedPage = await _cacheService.GetAsync<HomePageDto>(CacheKeys.HomePage);
            if (cachedPage != null)
            {
                return Result<HomePageDto>.Success(cachedPage);
            }

            var page = await _unitOfWork.Pages.GetByPageTypeAsync(PageType.Home);
            if (page == null) return Result<HomePageDto>.Failure("Home page content not found.");

            var homePageDto = new HomePageDto
            {
                PageInfo = _mapper.Map<PageDto>(page)
            };

            // Get other data
            var services = await _unitOfWork.Services.GetFeaturedServicesAsync();
            homePageDto.FeaturedServices = _mapper.Map<List<ServiceDto>>(services.Take(4));

            var caseStudies = await _unitOfWork.CaseStudies.GetFeaturedCaseStudiesAsync();
            homePageDto.FeaturedCaseStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies.Take(2));

            var testimonial = await _unitOfWork.Testimonials.GetRandomTestimonialAsync();
            homePageDto.FeaturedTestimonial = testimonial != null ? _mapper.Map<TestimonialDto>(testimonial) : null;

            var partners = await _unitOfWork.Partners.GetActivePartnersAsync();
            homePageDto.Partners = _mapper.Map<List<PartnerDto>>(partners.Take(6));

            var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
            homePageDto.CompanyInfo = _mapper.Map<CompanyInfoDto>(companyInfo);

            await _cacheService.SetAsync(CacheKeys.HomePage, homePageDto, TimeSpan.FromMinutes(30));

            return Result<HomePageDto>.Success(homePageDto);
        }
    }
}