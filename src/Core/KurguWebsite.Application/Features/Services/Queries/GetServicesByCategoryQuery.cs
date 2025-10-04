// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServicesByCategoryQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetServicesByCategoryQuery : IRequest<Result<List<ServiceDto>>>
    {
        public ServiceCategory Category { get; set; }
    }

    public class GetServicesByCategoryQueryHandler
        : IRequestHandler<GetServicesByCategoryQuery, Result<List<ServiceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetServicesByCategoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ServiceDto>>> Handle(
            GetServicesByCategoryQuery request,
            CancellationToken ct)
        {
            var spec = new ServicesByCategorySpecification(request.Category);
            var services = await _unitOfWork.Services.ListAsync(spec, ct);
            var mappedServices = _mapper.Map<List<ServiceDto>>(services);

            return Result<List<ServiceDto>>.Success(mappedServices);
        }
    }
}