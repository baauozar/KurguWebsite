using AutoMapper;
using AutoMapper.QueryableExtensions;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using MediatR;
using System.Linq;
using System.Linq.Dynamic.Core; // Add NuGet package: System.Linq.Dynamic.Core
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class SearchCaseStudiesQuery : IRequest<Result<PaginatedList<CaseStudyDto>>>
    {
        public QueryParameters Params { get; set; }
    }

    public class SearchCaseStudiesQueryHandler : IRequestHandler<SearchCaseStudiesQuery, Result<PaginatedList<CaseStudyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SearchCaseStudiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<CaseStudyDto>>> Handle(SearchCaseStudiesQuery request, CancellationToken cancellationToken)
        {
            var caseStudiesQuery = (await _unitOfWork.CaseStudies.GetAsync(
                predicate: null,
                orderBy: null,
                includeString: null,
                disableTracking: true)).AsQueryable();

            // Search Filter
            if (!string.IsNullOrWhiteSpace(request.Params.SearchTerm))
            {
                var term = request.Params.SearchTerm.Trim().ToLower();
                caseStudiesQuery = caseStudiesQuery.Where(cs =>
                    cs.Title.ToLower().Contains(term) ||
                    cs.ClientName.ToLower().Contains(term) ||
                    cs.Description.ToLower().Contains(term)
                );
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(request.Params.SortColumn))
            {
                var sortOrder = request.Params.SortOrder?.ToLower() == "desc" ? "descending" : "ascending";
                caseStudiesQuery = caseStudiesQuery.OrderBy($"{request.Params.SortColumn} {sortOrder}");
            }
            else
            {
                caseStudiesQuery = caseStudiesQuery.OrderBy(cs => cs.DisplayOrder).ThenByDescending(cs => cs.CompletedDate);
            }

            var paginatedList = await PaginatedList<CaseStudyDto>.CreateAsync(
                caseStudiesQuery.ProjectTo<CaseStudyDto>(_mapper.ConfigurationProvider),
                request.Params.PageNumber,
                request.Params.PageSize);

            return Result<PaginatedList<CaseStudyDto>>.Success(paginatedList);
        }
    }
}