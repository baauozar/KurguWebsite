// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/DeleteCaseStudyMetricCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Commands;

public class DeleteCaseStudyMetricCommand : IRequest<ControlResult>
{
    public Guid Id { get; set; }
}

public class DeleteCaseStudyMetricCommandHandler : IRequestHandler<DeleteCaseStudyMetricCommand, ControlResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCaseStudyMetricCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ControlResult> Handle(DeleteCaseStudyMetricCommand request, CancellationToken cancellationToken)
    {
        var caseStudyMetric = await _unitOfWork.CaseStudyMetrics.GetByIdAsync(request.Id);

        if (caseStudyMetric == null)
        {
            return ControlResult.Failure("Case Study Metric not found.");
        }

        await _unitOfWork.CaseStudyMetrics.DeleteAsync(caseStudyMetric);
        await _unitOfWork.CommitAsync();

        return ControlResult.Success();
    }
}