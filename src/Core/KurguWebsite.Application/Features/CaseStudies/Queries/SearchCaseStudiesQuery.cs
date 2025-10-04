// src/Core/KurguWebsite.Application/Features/CaseStudies/Queries/SearchCaseStudiesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class SearchCaseStudiesQuery : IRequest<Result<PaginatedList<CaseStudyDto>>>
    {
        public string? SearchTerm { get; set; }
        public string? Industry { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SearchCaseStudiesQueryHandler
        : IRequestHandler<SearchCaseStudiesQuery, Result<PaginatedList<CaseStudyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SearchCaseStudiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<CaseStudyDto>>> Handle(
            SearchCaseStudiesQuery request,
            CancellationToken cancellationToken)
        {
            var spec = new CaseStudySearchSpecification(
                searchTerm: request.SearchTerm,
                industry: request.Industry,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            var caseStudies = await _unitOfWork.CaseStudies.ListAsync(spec, cancellationToken);

            var countSpec = new CaseStudySearchSpecification(
                searchTerm: request.SearchTerm,
                industry: request.Industry,
                pageNumber: 1,
                pageSize: int.MaxValue);
            var totalCount = await _unitOfWork.CaseStudies.CountAsync(countSpec, cancellationToken);

            var mappedStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies);

            var paginatedList = new PaginatedList<CaseStudyDto>(
                mappedStudies,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedList<CaseStudyDto>>.Success(paginatedList);
        }
    }
}