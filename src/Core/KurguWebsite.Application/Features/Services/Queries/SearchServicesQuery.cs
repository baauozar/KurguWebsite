// src/Core/KurguWebsite.Application/Features/Services/Queries/SearchServicesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class SearchServicesQuery : IRequest<Result<PaginatedList<ServiceDto>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeInactive { get; set; } = false;
    }

    public class SearchServicesQueryHandler
        : IRequestHandler<SearchServicesQuery, Result<PaginatedList<ServiceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SearchServicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ServiceDto>>> Handle(
            SearchServicesQuery request,
            CancellationToken ct)
        {
            var spec = new ServiceSearchSpecification(
                searchTerm: request.SearchTerm,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                includeInactive: request.IncludeInactive);

            var services = await _unitOfWork.Services.ListAsync(spec, ct);

            var countSpec = new ServiceSearchSpecification(
                searchTerm: request.SearchTerm,
                pageNumber: 1,
                pageSize: int.MaxValue,
                includeInactive: request.IncludeInactive);
            var totalCount = await _unitOfWork.Services.CountAsync(countSpec, ct);

            var mappedServices = _mapper.Map<List<ServiceDto>>(services);

            var paginatedList = new PaginatedList<ServiceDto>(
                mappedServices,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedList<ServiceDto>>.Success(paginatedList);
        }
    }
}