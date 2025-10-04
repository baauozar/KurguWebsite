// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/RestoreCaseStudyMetricCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class RestoreCaseStudyMetricCommand : IRequest<Result<CaseStudyMetricDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreCaseStudyMetricCommandHandler
        : IRequestHandler<RestoreCaseStudyMetricCommand, Result<CaseStudyMetricDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RestoreCaseStudyMetricCommandHandler> _logger;

        public RestoreCaseStudyMetricCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<RestoreCaseStudyMetricCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CaseStudyMetricDto>> Handle(
            RestoreCaseStudyMetricCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var entity = await _unitOfWork.CaseStudyMetrics.GetByIdIncludingDeletedAsync(request.Id);
                    if (entity is null)
                    {
                        return Result<CaseStudyMetricDto>.Failure(
                            "Case study metric not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    if (entity.IsDeleted)
                    {
                        await _unitOfWork.CaseStudyMetrics.RestoreAsync(entity);
                        entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                        entity.LastModifiedDate = DateTime.UtcNow;

                        await _unitOfWork.CommitAsync(ct);
                        await _unitOfWork.CommitTransactionAsync(ct);

                        _logger.LogInformation("Case study metric restored: Id={MetricId}, CaseStudyId={CaseStudyId}",
                            request.Id, entity.CaseStudyId);

                        // Get the case study to invalidate its cache
                        var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(entity.CaseStudyId);
                        if (caseStudy != null)
                        {
                            await _mediator.Publish(
                                new CacheInvalidationEvent(
                                    CacheKeys.CaseStudies,
                                    CacheKeys.FeaturedCaseStudies,
                                    string.Format(CacheKeys.CaseStudyBySlug, caseStudy.Slug)),
                                ct);
                        }
                    }

                    return Result<CaseStudyMetricDto>.Success(_mapper.Map<CaseStudyMetricDto>(entity));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error restoring case study metric: {MetricId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<CaseStudyMetricDto>.Failure($"Failed to restore case study metric: {ex.Message}");
                }
            }
        }
    }
}