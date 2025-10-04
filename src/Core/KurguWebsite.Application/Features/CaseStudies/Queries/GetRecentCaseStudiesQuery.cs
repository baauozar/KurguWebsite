// src/Core/KurguWebsite.Application/Features/CaseStudies/Queries/GetRecentCaseStudiesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class GetRecentCaseStudiesQuery : IRequest<Result<List<CaseStudyDto>>>
    {
        public int Count { get; set; } = 5;
    }

    public class GetRecentCaseStudiesQueryHandler
        : IRequestHandler<GetRecentCaseStudiesQuery, Result<List<CaseStudyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetRecentCaseStudiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<CaseStudyDto>>> Handle(
            GetRecentCaseStudiesQuery request,
            CancellationToken ct)
        {
            var spec = new RecentCaseStudiesSpecification(request.Count);
            var caseStudies = await _unitOfWork.CaseStudies.ListAsync(spec, ct);
            var mappedStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies);

            return Result<List<CaseStudyDto>>.Success(mappedStudies);
        }
    }
}