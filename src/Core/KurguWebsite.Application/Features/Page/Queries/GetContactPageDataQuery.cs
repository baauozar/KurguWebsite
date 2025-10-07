// src/Core/KurguWebsite.Application/Features/Page/Queries/GetContactPageDataQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Domain.Enums;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class GetContactPageDataQuery : IRequest<Result<ContactPageDto>> { }

    public class GetContactPageDataQueryHandler
        : IRequestHandler<GetContactPageDataQuery, Result<ContactPageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetContactPageDataQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<ContactPageDto>> Handle(
            GetContactPageDataQuery request,
            CancellationToken cancellationToken)
        {
            // Check cache first
            var cachedPage = await _cacheService.GetAsync<ContactPageDto>(CacheKeys.ContactPage);
            if (cachedPage != null)
            {
                return Result<ContactPageDto>.Success(cachedPage);
            }

            // Get Contact page content
            var page = await _unitOfWork.Pages.GetByPageTypeAsync(PageType.Contact);
            if (page == null)
                return Result<ContactPageDto>.Failure("Contact page content not found.");

            var contactPageDto = new ContactPageDto
            {
                PageInfo = _mapper.Map<PageDto>(page)
            };

            // Get company info (for contact details and address)
            var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
            if (companyInfo != null)
            {
                contactPageDto.CompanyInfo = _mapper.Map<CompanyInfoDto>(companyInfo);
            }

            // Cache the result
            await _cacheService.SetAsync(
                CacheKeys.ContactPage,
                contactPageDto,
                TimeSpan.FromMinutes(30));

            return Result<ContactPageDto>.Success(contactPageDto);
        }
    }
}