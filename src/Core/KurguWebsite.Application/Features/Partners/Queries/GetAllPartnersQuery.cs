// src/Core/KurguWebsite.Application/Features/Partners/Queries/GetAllPartnersQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Partners.Queries
{
    public class GetAllPartnersQuery : IRequest<Result<List<PartnerDto>>> { }

    public class GetAllPartnersQueryHandler
        : IRequestHandler<GetAllPartnersQuery, Result<List<PartnerDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetAllPartnersQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<List<PartnerDto>>> Handle(
            GetAllPartnersQuery request,
            CancellationToken ct)
        {
            var cachedPartners = await _cacheService.GetAsync<List<PartnerDto>>(CacheKeys.ActivePartners);
            if (cachedPartners != null)
            {
                return Result<List<PartnerDto>>.Success(cachedPartners);
            }

            var spec = new ActivePartnersSpecification();
            var partners = await _unitOfWork.Partners.ListAsync(spec, ct);
            var mappedPartners = _mapper.Map<List<PartnerDto>>(partners);

            await _cacheService.SetAsync(
                CacheKeys.ActivePartners,
                mappedPartners,
                TimeSpan.FromMinutes(30));

            return Result<List<PartnerDto>>.Success(mappedPartners);
        }
    }
}