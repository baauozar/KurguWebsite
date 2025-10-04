// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServiceBySlugQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetServiceBySlugQuery : IRequest<Result<ServiceDto>>
    {
        public string Slug { get; set; } = string.Empty;
    }

    public class GetServiceBySlugQueryHandler
        : IRequestHandler<GetServiceBySlugQuery, Result<ServiceDto>>
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

        public async Task<Result<ServiceDto>> Handle(
            GetServiceBySlugQuery request,
            CancellationToken ct)
        {
            var cacheKey = string.Format(CacheKeys.ServiceBySlug, request.Slug);
            var cachedService = await _cacheService.GetAsync<ServiceDto>(cacheKey);

            if (cachedService != null)
            {
                return Result<ServiceDto>.Success(cachedService);
            }

            var spec = new ServiceBySlugSpecification(request.Slug);
            var service = await _unitOfWork.Services.GetBySpecAsync(spec, ct);

            if (service == null)
            {
                return Result<ServiceDto>.Failure(
                    "Service not found.",
                    ErrorCodes.EntityNotFound);
            }

            var mappedService = _mapper.Map<ServiceDto>(service);
            await _cacheService.SetAsync(cacheKey, mappedService, TimeSpan.FromMinutes(30));

            return Result<ServiceDto>.Success(mappedService);
        }
    }
}