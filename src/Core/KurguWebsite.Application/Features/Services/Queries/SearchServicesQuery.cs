// src/Core/KurguWebsite.Application/Features/Services/Queries/SearchServicesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Specifications;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
            CancellationToken cancellationToken)
        {
            // Use specification for search
            var spec = new ServiceSearchSpecification(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                request.IncludeInactive);

            // Get paginated results
            var services = await _unitOfWork.Services.ListAsync(spec, cancellationToken);

            // Get total count (without pagination)
            var countSpec = new ServiceSearchSpecification(
                request.SearchTerm,
                1,
                int.MaxValue,
                request.IncludeInactive);
            var totalCount = await _unitOfWork.Services.CountAsync(countSpec, cancellationToken);

            // Map to DTOs
            var mappedServices = _mapper.Map<List<ServiceDto>>(services);

            // Create paginated list
            var paginatedList = new PaginatedList<ServiceDto>(
                mappedServices,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedList<ServiceDto>>.Success(paginatedList);
        }
    }
}