using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using Microsoft.Extensions.Logging;


namespace KurguWebsite.Application.Services.Concrete
{
    public class PageService : IPageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ISeoService _seoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<PageService> _logger;

        public PageService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ISeoService seoService,
            ICurrentUserService currentUserService,
            ILogger<PageService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _seoService = seoService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<PageDto>> GetBySlugAsync(string slug)
        {
            try
            {
                var page = await _unitOfWork.Pages.GetBySlugAsync(slug);

                if (page == null)
                    return Result<PageDto>.Failure($"Page with slug '{slug}' not found");

                var pageDto = _mapper.Map<PageDto>(page);
                return Result<PageDto>.Success(pageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving page by slug {Slug}", slug);
                return Result<PageDto>.Failure($"An error occurred while retrieving the page");
            }
        }

        public async Task<Result<PageDto>> GetByTypeAsync(PageType pageType)
        {
            try
            {
                var page = await _unitOfWork.Pages.GetByPageTypeAsync(pageType);

                if (page == null)
                {
                    // Create default page if not exists
                    page = Page.Create(pageType.ToString(), pageType);
                    page.SetCreatedBy("System");
                    await _unitOfWork.Pages.AddAsync(page);
                    await _unitOfWork.CommitAsync();
                }

                var pageDto = _mapper.Map<PageDto>(page);
                return Result<PageDto>.Success(pageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving page by type {PageType}", pageType);
                return Result<PageDto>.Failure($"An error occurred while retrieving the page");
            }
        }

        public async Task<Result<HomePageDto>> GetHomePageDataAsync()
        {
            try
            {
                var cachedHomePage = await _cacheService.GetAsync<HomePageDto>(CacheKeys.HomePage);

                if (cachedHomePage != null)
                    return Result<HomePageDto>.Success(cachedHomePage);

                // Get page info
                var page = await _unitOfWork.Pages.GetByPageTypeAsync(PageType.Home);
                if (page == null)
                {
                    page = Page.Create("Home", PageType.Home);
                    page.UpdateHeroSection(
                        "ARE YOU LOOKING FOR SUPERIOR TECH IT SUPPORT?",
                        null,
                        "We provide affordable, highly responsive IT Support and Services for small and medium businesses.",
                        "/img/demos/it-services/backgrounds/bg-full.jpg",
                        "GET STARTED NOW",
                        "#services"
                    );
                    page.SetCreatedBy("System");
                    await _unitOfWork.Pages.AddAsync(page);
                    await _unitOfWork.CommitAsync();
                }

                var homePageDto = new HomePageDto
                {
                    PageInfo = _mapper.Map<PageDto>(page)
                };

                // Get featured services
                var services = await _unitOfWork.Services.GetFeaturedServicesAsync();
                homePageDto.FeaturedServices = _mapper.Map<List<ServiceDto>>(services.Take(4));

                // Get featured case studies
                var caseStudies = await _unitOfWork.CaseStudies.GetFeaturedCaseStudiesAsync();
                homePageDto.FeaturedCaseStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies.Take(2));

                // Get random testimonial
                var testimonial = await _unitOfWork.Testimonials.GetRandomTestimonialAsync();
                homePageDto.FeaturedTestimonial = testimonial != null ? _mapper.Map<TestimonialDto>(testimonial) : null;

                // Get process steps
                var processSteps = await _unitOfWork.ProcessSteps.GetActiveStepsOrderedAsync();
                homePageDto.ProcessSteps = _mapper.Map<List<ProcessStepDto>>(processSteps);

                // Get partners
                var partners = await _unitOfWork.Partners.GetActivePartnersAsync();
                homePageDto.Partners = _mapper.Map<List<PartnerDto>>(partners.Take(6));

                // Get company info
                var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
                homePageDto.CompanyInfo = _mapper.Map<CompanyInfoDto>(companyInfo);

                await _cacheService.SetAsync(CacheKeys.HomePage, homePageDto, TimeSpan.FromMinutes(30));

                return Result<HomePageDto>.Success(homePageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving home page data");
                return Result<HomePageDto>.Failure($"An error occurred while loading the home page");
            }
        }

        public async Task<Result<AboutPageDto>> GetAboutPageDataAsync()
        {
            try
            {
                var cachedAboutPage = await _cacheService.GetAsync<AboutPageDto>(CacheKeys.AboutPage);

                if (cachedAboutPage != null)
                    return Result<AboutPageDto>.Success(cachedAboutPage);

                // Get page info
                var page = await _unitOfWork.Pages.GetByPageTypeAsync(PageType.About);
                if (page == null)
                {
                    page = Page.Create("About Us", PageType.About);
                    page.UpdateHeroSection(
                        "ABOUT US",
                        null,
                        "Learn more about our company and team",
                        null,
                        null,
                        null
                    );
                    page.SetCreatedBy("System");
                    await _unitOfWork.Pages.AddAsync(page);
                    await _unitOfWork.CommitAsync();
                }

                var aboutPageDto = new AboutPageDto
                {
                    PageInfo = _mapper.Map<PageDto>(page)
                };

                // Get company info
                var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
                aboutPageDto.CompanyInfo = _mapper.Map<CompanyInfoDto>(companyInfo);

                // Get testimonials
                var testimonials = await _unitOfWork.Testimonials.GetActiveTestimonialsAsync();
                aboutPageDto.Testimonials = _mapper.Map<List<TestimonialDto>>(testimonials);

                // Get partners
                var partners = await _unitOfWork.Partners.GetActivePartnersAsync();
                aboutPageDto.Partners = _mapper.Map<List<PartnerDto>>(partners);

                await _cacheService.SetAsync(CacheKeys.AboutPage, aboutPageDto, TimeSpan.FromMinutes(30));

                return Result<AboutPageDto>.Success(aboutPageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving about page data");
                return Result<AboutPageDto>.Failure($"An error occurred while loading the about page");
            }
        }

        public async Task<Result<ContactPageDto>> GetContactPageDataAsync()
        {
            try
            {
                var cachedContactPage = await _cacheService.GetAsync<ContactPageDto>(CacheKeys.ContactPage);

                if (cachedContactPage != null)
                    return Result<ContactPageDto>.Success(cachedContactPage);

                // Get page info
                var page = await _unitOfWork.Pages.GetByPageTypeAsync(PageType.Contact);
                if (page == null)
                {
                    page = Page.Create("Contact Us", PageType.Contact);
                    page.UpdateHeroSection(
                        "CONTACT US",
                        null,
                        "Get in touch with our team",
                        null,
                        null,
                        null
                    );
                    page.SetCreatedBy("System");
                    await _unitOfWork.Pages.AddAsync(page);
                    await _unitOfWork.CommitAsync();
                }

                var contactPageDto = new ContactPageDto
                {
                    PageInfo = _mapper.Map<PageDto>(page)
                };

                // Get company info
                var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
                contactPageDto.CompanyInfo = _mapper.Map<CompanyInfoDto>(companyInfo);

                await _cacheService.SetAsync(CacheKeys.ContactPage, contactPageDto, TimeSpan.FromMinutes(30));

                return Result<ContactPageDto>.Success(contactPageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact page data");
                return Result<ContactPageDto>.Failure($"An error occurred while loading the contact page");
            }
        }

        public async Task<Result<ServicesPageDto>> GetServicesPageDataAsync()
        {
            try
            {
                var cachedServicesPage = await _cacheService.GetAsync<ServicesPageDto>(CacheKeys.ServicesPage);

                if (cachedServicesPage != null)
                    return Result<ServicesPageDto>.Success(cachedServicesPage);

                // Get page info
                var page = await _unitOfWork.Pages.GetByPageTypeAsync(PageType.Services);
                if (page == null)
                {
                    page = Page.Create("IT Services", PageType.Services);
                    page.UpdateHeroSection(
                        "IT SERVICES",
                        "OUR EXPERTISE",
                        "World-Class Solutions for your Business",
                        null,
                        null,
                        null
                    );
                    page.SetCreatedBy("System");
                    await _unitOfWork.Pages.AddAsync(page);
                    await _unitOfWork.CommitAsync();
                }

                var servicesPageDto = new ServicesPageDto
                {
                    PageInfo = _mapper.Map<PageDto>(page)
                };

                // Get all active services
                var services = await _unitOfWork.Services.GetActiveServicesAsync();
                servicesPageDto.Services = _mapper.Map<List<ServiceDto>>(services);

                // Get process steps
                var processSteps = await _unitOfWork.ProcessSteps.GetActiveStepsOrderedAsync();
                servicesPageDto.ProcessSteps = _mapper.Map<List<ProcessStepDto>>(processSteps);

                await _cacheService.SetAsync(CacheKeys.ServicesPage, servicesPageDto, TimeSpan.FromMinutes(30));

                return Result<ServicesPageDto>.Success(servicesPageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services page data");
                return Result<ServicesPageDto>.Failure($"An error occurred while loading the services page");
            }
        }

        public async Task<Result<PageDto>> UpdatePageAsync(Guid id, UpdatePageDto dto)
        {
            try
            {
                var page = await _unitOfWork.Pages.GetByIdAsync(id);

                if (page == null)
                    return Result<PageDto>.Failure($"Page with id {id} not found");

                page.UpdateContent(dto.Content);
                page.UpdateSeo(dto.MetaTitle, dto.MetaDescription, dto.MetaKeywords);
                page.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Pages.UpdateAsync(page);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearPageCacheAsync();

                var pageDto = _mapper.Map<PageDto>(page);
                return Result<PageDto>.Success(pageDto, "Page updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId}", id);
                return Result<PageDto>.Failure($"An error occurred while updating the page");
            }
        }

        public async Task<Result<bool>> UpdateHeroSectionAsync(Guid id, UpdateHeroSectionDto dto)
        {
            try
            {
                var page = await _unitOfWork.Pages.GetByIdAsync(id);

                if (page == null)
                    return Result<bool>.Failure($"Page with id {id} not found");

                page.UpdateHeroSection(
                    dto.HeroTitle,
                    dto.HeroSubtitle,
                    dto.HeroDescription,
                    dto.HeroBackgroundImage,
                    dto.HeroButtonText,
                    dto.HeroButtonUrl);

                page.SetModifiedBy(_currentUserService.UserId ?? "System");

                await _unitOfWork.Pages.UpdateAsync(page);
                await _unitOfWork.CommitAsync();

                // Clear cache
                await ClearPageCacheAsync();

                return Result<bool>.Success(true, "Hero section updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hero section for page {PageId}", id);
                return Result<bool>.Failure($"An error occurred while updating the hero section");
            }
        }

        private async Task ClearPageCacheAsync()
        {
            await _cacheService.RemoveAsync(CacheKeys.HomePage);
            await _cacheService.RemoveAsync(CacheKeys.AboutPage);
            await _cacheService.RemoveAsync(CacheKeys.ServicesPage);
            await _cacheService.RemoveAsync(CacheKeys.ContactPage);
        }
    }
}