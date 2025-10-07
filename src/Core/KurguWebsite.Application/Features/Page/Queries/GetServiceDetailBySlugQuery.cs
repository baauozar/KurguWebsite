// src/Core/KurguWebsite.Application/Features/Service/Queries/GetServiceDetailBySlugQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetServiceDetailBySlugQuery : IRequest<Result<ServiceDetailDto>>
    {
        public string Slug { get; set; } = string.Empty;
    }

    public class GetServiceDetailBySlugQueryHandler
        : IRequestHandler<GetServiceDetailBySlugQuery, Result<ServiceDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetServiceDetailBySlugQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<ServiceDetailDto>> Handle(
            GetServiceDetailBySlugQuery request,
            CancellationToken cancellationToken)
        {
            // Check cache
            var cacheKey = string.Format(CacheKeys.ServiceBySlug, request.Slug);
            var cachedService = await _cacheService.GetAsync<ServiceDetailDto>(cacheKey);
            if (cachedService != null)
            {
                return Result<ServiceDetailDto>.Success(cachedService);
            }

            // Get service by slug with features
            var spec = new ServiceBySlugSpecification(request.Slug);
            var service = await _unitOfWork.Services.GetBySpecAsync(spec, cancellationToken);

            if (service == null)
            {
                return Result<ServiceDetailDto>.Failure(
                    "Service not found.",
                    ErrorCodes.EntityNotFound);
            }

            var serviceDetailDto = _mapper.Map<ServiceDetailDto>(service);

            // Get related case studies
            var caseStudiesSpec = new CaseStudiesByServiceSpecification(service.Id);
            var caseStudies = await _unitOfWork.CaseStudies.ListAsync(caseStudiesSpec, cancellationToken);
            serviceDetailDto.RelatedCaseStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies.Take(3));

            // Get other services (exclude current)
            var otherServices = await _unitOfWork.Services.GetActiveServicesAsync();
            serviceDetailDto.OtherServices = _mapper.Map<List<ServiceDto>>(
                otherServices.Where(s => s.Id != service.Id).Take(4));

            // Cache the result
            await _cacheService.SetAsync(cacheKey, serviceDetailDto, TimeSpan.FromMinutes(30));

            return Result<ServiceDetailDto>.Success(serviceDetailDto);
        }
    }
}