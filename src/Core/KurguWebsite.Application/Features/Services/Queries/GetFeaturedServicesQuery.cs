// src/Core/KurguWebsite.Application/Features/Services/Queries/GetFeaturedServicesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetFeaturedServicesQuery : IRequest<Result<List<ServiceDto>>> { }

    public class GetFeaturedServicesQueryHandler
        : IRequestHandler<GetFeaturedServicesQuery, Result<List<ServiceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetFeaturedServicesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<List<ServiceDto>>> Handle(
            GetFeaturedServicesQuery request,
            CancellationToken ct)
        {
            var cachedServices = await _cacheService.GetAsync<List<ServiceDto>>(
                CacheKeys.FeaturedServices);

            if (cachedServices != null)
            {
                return Result<List<ServiceDto>>.Success(cachedServices);
            }

            var spec = new ActiveFeaturedServicesSpecification();
            var services = await _unitOfWork.Services.ListAsync(spec, ct);
            var mappedServices = _mapper.Map<List<ServiceDto>>(services);

            await _cacheService.SetAsync(
                CacheKeys.FeaturedServices,
                mappedServices,
                TimeSpan.FromMinutes(30));

            return Result<List<ServiceDto>>.Success(mappedServices);
        }
    }
}