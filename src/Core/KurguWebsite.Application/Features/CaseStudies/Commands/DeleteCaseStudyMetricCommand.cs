using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;

public sealed class DeleteCaseStudyMetricCommand : IRequest<ControlResult>
{
    public Guid Id { get; set; }
}

public sealed class DeleteCaseStudyMetricCommandHandler : IRequestHandler<DeleteCaseStudyMetricCommand, ControlResult>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteCaseStudyMetricCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ControlResult> Handle(DeleteCaseStudyMetricCommand request, CancellationToken ct)
    {
        var metric = await _uow.CaseStudyMetrics.GetByIdAsync(request.Id);
        if (metric is null) return ControlResult.Failure("Metric not found.");

        metric.SoftDelete(_currentUser.UserId ?? "System");
        await _uow.CaseStudyMetrics.UpdateAsync(metric);
        await _uow.CommitAsync();

        return ControlResult.Success();
    }
}
