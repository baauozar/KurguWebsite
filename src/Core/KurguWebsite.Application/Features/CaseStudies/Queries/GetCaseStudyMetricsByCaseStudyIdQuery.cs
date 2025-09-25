// src/Core/KurguWebsite.Application/Features/CaseStudies/Queries/GetCaseStudyMetricsByCaseStudyIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Queries;

public class GetCaseStudyMetricsByCaseStudyIdQuery : IRequest<Result<List<CaseStudyMetricDto>>>
{
    public Guid CaseStudyId { get; set; }
}

public class GetCaseStudyMetricsByCaseStudyIdQueryHandler : IRequestHandler<GetCaseStudyMetricsByCaseStudyIdQuery, Result<List<CaseStudyMetricDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCaseStudyMetricsByCaseStudyIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<CaseStudyMetricDto>>> Handle(GetCaseStudyMetricsByCaseStudyIdQuery request, CancellationToken cancellationToken)
    {
        var caseStudyMetrics = await _unitOfWork.CaseStudyMetrics.GetByCaseStudyIdAsync(request.CaseStudyId);
        var caseStudyMetricDtos = _mapper.Map<List<CaseStudyMetricDto>>(caseStudyMetrics);
        return Result<List<CaseStudyMetricDto>>.Success(caseStudyMetricDtos);
    }
}