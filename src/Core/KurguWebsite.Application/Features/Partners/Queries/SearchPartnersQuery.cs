// src/Core/KurguWebsite.Application/Features/Partners/Queries/SearchPartnersQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Partners.Queries
{
    public class SearchPartnersQuery : IRequest<Result<PaginatedList<PartnerDto>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SearchPartnersQueryHandler
        : IRequestHandler<SearchPartnersQuery, Result<PaginatedList<PartnerDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SearchPartnersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<PartnerDto>>> Handle(
            SearchPartnersQuery request,
            CancellationToken ct)
        {
            var spec = new PartnerSearchSpecification(
                searchTerm: request.SearchTerm,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            var partners = await _unitOfWork.Partners.ListAsync(spec, ct);

            var countSpec = new PartnerSearchSpecification(
                searchTerm: request.SearchTerm,
                pageNumber: 1,
                pageSize: int.MaxValue);
            var totalCount = await _unitOfWork.Partners.CountAsync(countSpec, ct);

            var mappedPartners = _mapper.Map<List<PartnerDto>>(partners);

            var paginatedList = new PaginatedList<PartnerDto>(
                mappedPartners,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedList<PartnerDto>>.Success(paginatedList);
        }
    }
}