// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/DeleteCaseStudyMetricCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class DeleteCaseStudyMetricCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCaseStudyMetricCommandHandler
        : IRequestHandler<DeleteCaseStudyMetricCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteCaseStudyMetricCommandHandler> _logger;

        public DeleteCaseStudyMetricCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteCaseStudyMetricCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ControlResult> Handle(
            DeleteCaseStudyMetricCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var metric = await _unitOfWork.CaseStudyMetrics.GetByIdAsync(request.Id);
                    if (metric is null)
                    {
                        return ControlResult.Failure("Metric not found.");
                    }

                    metric.SoftDelete(_currentUserService.UserId ?? "System");
                    await _unitOfWork.CaseStudyMetrics.UpdateAsync(metric);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Case study metric deleted: Id={MetricId}", request.Id);

                    return ControlResult.Success();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting case study metric: {MetricId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    throw;
                }
            }
        }
    }
}