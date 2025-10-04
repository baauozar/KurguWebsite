// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServiceFeaturesByServiceIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries;

public class GetServiceFeaturesByServiceIdQuery : IRequest<Result<List<ServiceFeatureDto>>>
{
    public Guid ServiceId { get; set; }
}

public class GetServiceFeaturesByServiceIdQueryHandler
    : IRequestHandler<GetServiceFeaturesByServiceIdQuery, Result<List<ServiceFeatureDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetServiceFeaturesByServiceIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ServiceFeatureDto>>> Handle(
        GetServiceFeaturesByServiceIdQuery request,
        CancellationToken ct)
    {
        var spec = new FeaturesByServiceSpecification(request.ServiceId);
        var features = await _unitOfWork.ServiceFeatures.ListAsync(spec, ct);
        var mappedFeatures = _mapper.Map<List<ServiceFeatureDto>>(features);

        return Result<List<ServiceFeatureDto>>.Success(mappedFeatures);
    }
}
