// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServiceBySlugQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Specifications;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetServiceBySlugQuery : IRequest<Result<ServiceDetailDto>>
    {
        public string Slug { get; set; } = string.Empty;
    }

    public class GetServiceBySlugQueryHandler
        : IRequestHandler<GetServiceBySlugQuery, Result<ServiceDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetServiceBySlugQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<ServiceDetailDto>> Handle(
            GetServiceBySlugQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = string.Format(CacheKeys.ServiceBySlug, request.Slug);

            // Check cache
            var cachedService = await _cacheService.GetAsync<ServiceDetailDto>(cacheKey);
            if (cachedService != null)
            {
                return Result<ServiceDetailDto>.Success(cachedService);
            }

            // Use specification to get service with all related data
            var spec = new ServiceBySlugSpecification(request.Slug);
            var service = await _unitOfWork.Services.GetBySpecAsync(spec, cancellationToken);

            if (service == null)
            {
                return Result<ServiceDetailDto>.Failure(
                    "Service not found.",
                    "NOT_FOUND");
            }

            var mappedService = _mapper.Map<ServiceDetailDto>(service);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, mappedService, TimeSpan.FromMinutes(30));

            return Result<ServiceDetailDto>.Success(mappedService);
        }
    }
}