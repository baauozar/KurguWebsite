// src/Core/KurguWebsite.Application/Features/CaseStudies/Queries/GetCaseStudyMetricByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Queries;

public class GetCaseStudyMetricByIdQuery : IRequest<Result<CaseStudyMetricDto>>
{
    public Guid Id { get; set; }
}

public class GetCaseStudyMetricByIdQueryHandler : IRequestHandler<GetCaseStudyMetricByIdQuery, Result<CaseStudyMetricDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCaseStudyMetricByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CaseStudyMetricDto>> Handle(GetCaseStudyMetricByIdQuery request, CancellationToken cancellationToken)
    {
        var caseStudyMetric = await _unitOfWork.CaseStudyMetrics.GetByIdAsync(request.Id);

        if (caseStudyMetric == null)
        {
            return Result<CaseStudyMetricDto>.Failure("Case Study Metric not found.");
        }

        var caseStudyMetricDto = _mapper.Map<CaseStudyMetricDto>(caseStudyMetric);
        return Result<CaseStudyMetricDto>.Success(caseStudyMetricDto);
    }
}