// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/CreateCaseStudyMetricCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    // OPTION 1: Don't inherit from DTO - Recommended for cleaner separation
    public class CreateCaseStudyMetricCommand : IRequest<Result<CaseStudyMetricDto>>
    {
        public Guid CaseStudyId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public string MetricValue { get; set; } = string.Empty;
        public string? Icon { get; set; }
    }


    public class CreateCaseStudyMetricCommandHandler
        : IRequestHandler<CreateCaseStudyMetricCommand, Result<CaseStudyMetricDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateCaseStudyMetricCommandHandler> _logger;
        private readonly IMediator _mediator;
        public CreateCaseStudyMetricCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<CreateCaseStudyMetricCommandHandler> logger,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result<CaseStudyMetricDto>> Handle(
            CreateCaseStudyMetricCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    // Validate CaseStudy exists
                    var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(request.CaseStudyId);
                    if (caseStudy == null)
                    {
                        return Result<CaseStudyMetricDto>.Failure(
                            "Case study not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    // Get the next display order
                    var existingMetrics = await _unitOfWork.CaseStudyMetrics
                        .GetByCaseStudyIdAsync(request.CaseStudyId);
                    var nextDisplayOrder = existingMetrics.Any()
                        ? existingMetrics.Max(m => m.DisplayOrder) + 1
                        : 1;

                    // Create the metric
                    var metric = CaseStudyMetric.Create(
                        request.CaseStudyId,
                        request.MetricName,
                        request.MetricValue,
                        request.Icon,
                        displayOrder: nextDisplayOrder
                    );

                    metric.CreatedBy = _currentUserService.UserId ?? "System";
                    metric.CreatedDate = DateTime.UtcNow;

                    await _unitOfWork.CaseStudyMetrics.AddAsync(metric);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Case study metric created: Id={MetricId}, CaseStudyId={CaseStudyId}, Name={MetricName}",
                        metric.Id, request.CaseStudyId, request.MetricName);

                    // Invalidate case study cache
                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.CaseStudies,
                            CacheKeys.FeaturedCaseStudies,
                            string.Format(CacheKeys.CaseStudyBySlug, caseStudy.Slug)),
                        ct);

                    return Result<CaseStudyMetricDto>.Success(_mapper.Map<CaseStudyMetricDto>(metric));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating case study metric for CaseStudyId: {CaseStudyId}",
                        request.CaseStudyId);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<CaseStudyMetricDto>.Failure($"Failed to create metric: {ex.Message}");
                }
            }
        }
    }
}