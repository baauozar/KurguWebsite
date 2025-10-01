// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/CreateCaseStudyMetricCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Entities;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Commands;

public class CreateCaseStudyMetricCommand : CreateCaseStudyMetricDto, IRequest<Result<CaseStudyMetricDto>>
{
    new public Guid CaseStudyId { get; set; }
}

public class CreateCaseStudyMetricCommandHandler : IRequestHandler<CreateCaseStudyMetricCommand, Result<CaseStudyMetricDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCaseStudyMetricCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CaseStudyMetricDto>> Handle(CreateCaseStudyMetricCommand request, CancellationToken cancellationToken)
    {
        var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(request.CaseStudyId);
        if (caseStudy == null)
        {
            return Result<CaseStudyMetricDto>.Failure("Case Study not found.");
        }

        var caseStudyMetric = CaseStudyMetric.Create(request.CaseStudyId,request.MetricName, request.MetricValue,request.Icon);

        await _unitOfWork.CaseStudyMetrics.AddAsync(caseStudyMetric);
        await _unitOfWork.CommitAsync();

        return Result<CaseStudyMetricDto>.Success(_mapper.Map<CaseStudyMetricDto>(caseStudyMetric));
    }
}