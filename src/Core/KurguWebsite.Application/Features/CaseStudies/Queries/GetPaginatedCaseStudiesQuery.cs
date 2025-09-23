using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class GetPaginatedCaseStudiesQuery : IRequest<Result<PaginatedList<CaseStudyDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetPaginatedCaseStudiesQueryHandler : IRequestHandler<GetPaginatedCaseStudiesQuery, Result<PaginatedList<CaseStudyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPaginatedCaseStudiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<CaseStudyDto>>> Handle(GetPaginatedCaseStudiesQuery request, CancellationToken cancellationToken)
        {
            var caseStudies = await _unitOfWork.CaseStudies.GetAsync(
                predicate: null,
                orderBy: q => q.OrderByDescending(d => d.CreatedDate),
                includeString: null,
                disableTracking: true);

            var mappedCaseStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies);

            //
            // --- THIS IS THE FIX ---
            // You must 'await' the async method to get the result.
            //
            var paginatedList = await PaginatedList<CaseStudyDto>.CreateAsync(mappedCaseStudies.AsQueryable(), request.PageNumber, request.PageSize);

            return Result<PaginatedList<CaseStudyDto>>.Success(paginatedList);
        }
    }
}