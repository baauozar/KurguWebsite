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
    private readonly ICurrentUserService _currentUserService;

    public CreateCaseStudyMetricCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CaseStudyMetricDto>> Handle(CreateCaseStudyMetricCommand request, CancellationToken cancellationToken)
    {
        var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(request.CaseStudyId);
        if (caseStudy == null)
        {
            return Result<CaseStudyMetricDto>.Failure("Case Study not found.");
        }

        // FIX: Pass displayOrder parameter
        var caseStudyMetric = CaseStudyMetric.Create(
            request.CaseStudyId,
            request.MetricName,
            request.MetricValue,
            request.Icon,
            displayOrder: 0
        );

        // Track who created
        caseStudyMetric.CreatedBy = _currentUserService.UserId ?? "System";
        caseStudyMetric.CreatedDate = DateTime.UtcNow;

        await _unitOfWork.CaseStudyMetrics.AddAsync(caseStudyMetric);
        await _unitOfWork.CommitAsync();

        return Result<CaseStudyMetricDto>.Success(_mapper.Map<CaseStudyMetricDto>(caseStudyMetric));
    }
}