// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServiceFeaturesByServiceIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
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
        CancellationToken cancellationToken)
    {
        var entities = await _unitOfWork.ServiceFeatures.GetByServiceIdAsync(request.ServiceId);
        var dtos = _mapper.Map<List<ServiceFeatureDto>>(entities);
        return Result<List<ServiceFeatureDto>>.Success(dtos);
    }
}
