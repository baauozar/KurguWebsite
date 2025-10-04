// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServicesWithCaseStudiesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetServicesWithCaseStudiesQuery : IRequest<Result<List<ServiceDto>>> { }

    public class GetServicesWithCaseStudiesQueryHandler
        : IRequestHandler<GetServicesWithCaseStudiesQuery, Result<List<ServiceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetServicesWithCaseStudiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ServiceDto>>> Handle(
            GetServicesWithCaseStudiesQuery request,
            CancellationToken ct)
        {
            var spec = new ServicesWithCaseStudiesSpecification();
            var services = await _unitOfWork.Services.ListAsync(spec, ct);
            var mappedServices = _mapper.Map<List<ServiceDto>>(services);

            return Result<List<ServiceDto>>.Success(mappedServices);
        }
    }
}