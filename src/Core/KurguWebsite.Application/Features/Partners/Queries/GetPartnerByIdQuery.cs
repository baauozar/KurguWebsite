// src/Core/KurguWebsite.Application/Features/Partners/Queries/GetPartnerByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using MediatR;

namespace KurguWebsite.Application.Features.Partners.Queries
{
    public class GetPartnerByIdQuery : IRequest<Result<PartnerDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetPartnerByIdQueryHandler
        : IRequestHandler<GetPartnerByIdQuery, Result<PartnerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPartnerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PartnerDto>> Handle(
            GetPartnerByIdQuery request,
            CancellationToken ct)
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(request.Id);
            if (partner == null)
            {
                return Result<PartnerDto>.Failure(
                    "Partner not found.",
                    ErrorCodes.EntityNotFound);
            }

            return Result<PartnerDto>.Success(_mapper.Map<PartnerDto>(partner));
        }
    }
}