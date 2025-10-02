// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/UpdateCaseStudyMetricCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class UpdateCaseStudyMetricCommand : IRequest<Result<CaseStudyMetricDto>>
    {
        public Guid Id { get; set; }
        public UpdateCaseStudyMetricDto UpdateCaseStudyMetricDto { get; set; } = null!;
    }

    public class UpdateCaseStudyMetricCommandHandler
        : IRequestHandler<UpdateCaseStudyMetricCommand, Result<CaseStudyMetricDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateCaseStudyMetricCommandHandler> _logger;

        public UpdateCaseStudyMetricCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<UpdateCaseStudyMetricCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CaseStudyMetricDto>> Handle(
            UpdateCaseStudyMetricCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var metric = await _unitOfWork.CaseStudyMetrics.GetByIdAsync(request.Id);
                    if (metric == null)
                    {
                        return Result<CaseStudyMetricDto>.Failure(
                            "Case study metric not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    metric.Update(
                        request.UpdateCaseStudyMetricDto.MetricName,
                        request.UpdateCaseStudyMetricDto.MetricValue,
                        request.UpdateCaseStudyMetricDto.Icon);

                    metric.LastModifiedBy = _currentUserService.UserId ?? "System";
                    metric.LastModifiedDate = DateTime.UtcNow;

                    await _unitOfWork.CaseStudyMetrics.UpdateAsync(metric);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Case study metric updated: Id={MetricId}", request.Id);

                    return Result<CaseStudyMetricDto>.Success(_mapper.Map<CaseStudyMetricDto>(metric));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating case study metric: {MetricId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<CaseStudyMetricDto>.Failure($"Failed to update metric: {ex.Message}");
                }
            }
        }
    }
}