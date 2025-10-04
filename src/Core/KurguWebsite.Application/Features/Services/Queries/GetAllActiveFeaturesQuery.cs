// src/Core/KurguWebsite.Application/Features/Services/Queries/GetAllActiveFeaturesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetAllActiveFeaturesQuery : IRequest<Result<List<ServiceFeatureDto>>> { }

    public class GetAllActiveFeaturesQueryHandler
        : IRequestHandler<GetAllActiveFeaturesQuery, Result<List<ServiceFeatureDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllActiveFeaturesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ServiceFeatureDto>>> Handle(
            GetAllActiveFeaturesQuery request,
            CancellationToken ct)
        {
            var spec = new ActiveFeaturesSpecification();
            var features = await _unitOfWork.ServiceFeatures.ListAsync(spec, ct);
            var mappedFeatures = _mapper.Map<List<ServiceFeatureDto>>(features);

            return Result<List<ServiceFeatureDto>>.Success(mappedFeatures);
        }
    }
}