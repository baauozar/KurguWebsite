// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/DeleteCaseStudyMetricCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Common;
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
            try
            {
                var metric = await _unitOfWork.CaseStudyMetrics.GetByIdAsync(request.Id);
                if (metric is null)
                {
                    return ControlResult.Failure("Metric not found.");
                }

                var caseStudyId = metric.CaseStudyId; // Store the parent ID before deleting

                // 1. Perform soft delete
                metric.SoftDelete(_currentUserService.UserId ?? "System");
                await _unitOfWork.CaseStudyMetrics.UpdateAsync(metric);

                // 2. Get remaining active metrics for the same case study
                var remainingMetrics = (await _unitOfWork.CaseStudyMetrics.GetAllAsync())
                                        .Where(m => m.IsActive && m.CaseStudyId == caseStudyId && m.Id != request.Id)
                                        .OrderBy(m => m.DisplayOrder)
                                        .ToList();

                // 3. Reorder the list
                remainingMetrics.Reorder();

                // 4. Mark reordered items for update
                foreach (var item in remainingMetrics)
                {
                    await _unitOfWork.CaseStudyMetrics.UpdateAsync(item);
                }

                // 5. Commit all changes at once
                await _unitOfWork.CommitAsync(ct);

                _logger.LogInformation("Case study metric soft-deleted and metrics reordered: Id={MetricId}", request.Id);
                return ControlResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting case study metric: {MetricId}", request.Id);
                return ControlResult.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}