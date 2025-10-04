// src/Core/KurguWebsite.Application/Features/Partners/Queries/GetPartnersByTypeQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Partners.Queries
{
    public class GetPartnersByTypeQuery : IRequest<Result<List<PartnerDto>>>
    {
        public PartnerType Type { get; set; }
    }

    public class GetPartnersByTypeQueryHandler
        : IRequestHandler<GetPartnersByTypeQuery, Result<List<PartnerDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPartnersByTypeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<PartnerDto>>> Handle(
            GetPartnersByTypeQuery request,
            CancellationToken ct)
        {
            var spec = new PartnersByTypeSpecification(request.Type);
            var partners = await _unitOfWork.Partners.ListAsync(spec, ct);
            var mappedPartners = _mapper.Map<List<PartnerDto>>(partners);

            return Result<List<PartnerDto>>.Success(mappedPartners);
        }
    }
}