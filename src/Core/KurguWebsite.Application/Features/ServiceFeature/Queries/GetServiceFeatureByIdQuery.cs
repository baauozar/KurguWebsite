// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServiceFeatureByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries;

public class GetServiceFeatureByIdQuery : IRequest<Result<ServiceFeatureDto>>
{
    public Guid Id { get; set; }
}

public class GetServiceFeatureByIdQueryHandler : IRequestHandler<GetServiceFeatureByIdQuery, Result<ServiceFeatureDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetServiceFeatureByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ServiceFeatureDto>> Handle(GetServiceFeatureByIdQuery request, CancellationToken cancellationToken)
    {
        var serviceFeature = await _unitOfWork.ServiceFeatures.GetByIdAsync(request.Id);

        if (serviceFeature == null)
        {
            return Result<ServiceFeatureDto>.Failure("Service Feature not found.");
        }

        var serviceFeatureDto = _mapper.Map<ServiceFeatureDto>(serviceFeature);
        return Result<ServiceFeatureDto>.Success(serviceFeatureDto);
    }
}