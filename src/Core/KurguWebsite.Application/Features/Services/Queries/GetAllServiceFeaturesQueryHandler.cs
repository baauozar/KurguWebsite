// src/Core/KurguWebsite.Application/Features/Services/Queries/GetAllServiceFeaturesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries;

public class GetAllServiceFeaturesQuery : IRequest<Result<List<ServiceFeatureDto>>> { }

public class GetAllServiceFeaturesQueryHandler
    : IRequestHandler<GetAllServiceFeaturesQuery, Result<List<ServiceFeatureDto>>>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public GetAllServiceFeaturesQueryHandler(IUnitOfWork uow, IMapper mapper)
    { _uow = uow; _mapper = mapper; }

    public async Task<Result<List<ServiceFeatureDto>>> Handle(GetAllServiceFeaturesQuery _, CancellationToken ct)
    {
        var entities = await _uow.ServiceFeatures.GetAllAsync();
        var dtos = _mapper.Map<List<ServiceFeatureDto>>(entities);
        return Result<List<ServiceFeatureDto>>.Success(dtos);
    }
}