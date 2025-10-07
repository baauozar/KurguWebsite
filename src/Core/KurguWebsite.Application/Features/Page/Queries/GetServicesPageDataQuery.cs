// src/Core/KurguWebsite.Application/Features/Page/Queries/GetServicesPageDataQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Domain.Enums;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class GetServicesPageDataQuery : IRequest<Result<ServicesPageDto>> { }

    public class GetServicesPageDataQueryHandler : IRequestHandler<GetServicesPageDataQuery, Result<ServicesPageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetServicesPageDataQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<ServicesPageDto>> Handle(
            GetServicesPageDataQuery request,
            CancellationToken cancellationToken)
        {
            // Check cache first
            var cachedPage = await _cacheService.GetAsync<ServicesPageDto>(CacheKeys.ServicesPage);
            if (cachedPage != null)
            {
                return Result<ServicesPageDto>.Success(cachedPage);
            }

            // Get Services page content
            var page = await _unitOfWork.Pages.GetByPageTypeAsync(PageType.Services);
            if (page == null)
                return Result<ServicesPageDto>.Failure("Services page content not found.");

            var servicesPageDto = new ServicesPageDto
            {
                PageInfo = _mapper.Map<PageDto>(page)
            };

            // Get all active services
            var services = await _unitOfWork.Services.GetActiveServicesAsync();
            servicesPageDto.Services = _mapper.Map<List<ServiceDto>>(services);

            // Replace this line:
            // var processSteps = await _unitOfWork.ProcessSteps.GetActiveProcessStepsAsync();

            // With this line:
            var processSteps = await _unitOfWork.ProcessSteps.GetActiveStepsOrderedAsync();
            servicesPageDto.ProcessSteps = _mapper.Map<List<ProcessStepDto>>(processSteps);

            // Cache the result
            await _cacheService.SetAsync(
                CacheKeys.ServicesPage,
                servicesPageDto,
                TimeSpan.FromMinutes(30));

            return Result<ServicesPageDto>.Success(servicesPageDto);
        }
    }
}