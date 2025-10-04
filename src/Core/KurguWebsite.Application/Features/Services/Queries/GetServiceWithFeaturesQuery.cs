// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServiceWithFeaturesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetServiceWithFeaturesQuery : IRequest<Result<ServiceDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetServiceWithFeaturesQueryHandler
        : IRequestHandler<GetServiceWithFeaturesQuery, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetServiceWithFeaturesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ServiceDto>> Handle(
            GetServiceWithFeaturesQuery request,
            CancellationToken ct)
        {
            var spec = new ServiceByIdWithFeaturesSpecification(request.Id);
            var service = await _unitOfWork.Services.GetBySpecAsync(spec, ct);

            if (service == null)
            {
                return Result<ServiceDto>.Failure(
                    "Service not found.",
                    ErrorCodes.EntityNotFound);
            }

            return Result<ServiceDto>.Success(_mapper.Map<ServiceDto>(service));
        }
    }
}