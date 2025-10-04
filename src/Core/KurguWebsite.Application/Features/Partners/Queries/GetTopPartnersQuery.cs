// src/Core/KurguWebsite.Application/Features/Partners/Queries/GetTopPartnersQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Partners.Queries
{
    public class GetTopPartnersQuery : IRequest<Result<List<PartnerDto>>>
    {
        public int Count { get; set; } = 10;
    }

    public class GetTopPartnersQueryHandler
        : IRequestHandler<GetTopPartnersQuery, Result<List<PartnerDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTopPartnersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<PartnerDto>>> Handle(
            GetTopPartnersQuery request,
            CancellationToken ct)
        {
            var spec = new TopPartnersSpecification(request.Count);
            var partners = await _unitOfWork.Partners.ListAsync(spec, ct);
            var mappedPartners = _mapper.Map<List<PartnerDto>>(partners);

            return Result<List<PartnerDto>>.Success(mappedPartners);
        }
    }
}