// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServiceByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetServiceByIdQuery : IRequest<Result<ServiceDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetServiceByIdQueryHandler
        : IRequestHandler<GetServiceByIdQuery, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetServiceByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<ServiceDto>> Handle(
            GetServiceByIdQuery request,
            CancellationToken ct)
        {
            var cacheKey = string.Format(CacheKeys.ServiceById, request.Id);
            var cachedService = await _cacheService.GetAsync<ServiceDto>(cacheKey);

            if (cachedService != null)
            {
                return Result<ServiceDto>.Success(cachedService);
            }

            var service = await _unitOfWork.Services.GetByIdAsync(request.Id);
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