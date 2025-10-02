// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/CreateCaseStudyMetricCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class CreateCaseStudyMetricCommand : CreateCaseStudyMetricDto,
        IRequest<Result<CaseStudyMetricDto>>
    {
        public new Guid CaseStudyId { get; set; }
    }

    public class CreateCaseStudyMetricCommandHandler
        : IRequestHandler<CreateCaseStudyMetricCommand, Result<CaseStudyMetricDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateCaseStudyMetricCommandHandler> _logger;

        public CreateCaseStudyMetricCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<CreateCaseStudyMetricCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CaseStudyMetricDto>> Handle(
            CreateCaseStudyMetricCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(request.CaseStudyId);
                    if (caseStudy == null)
                    {
                        return Result<CaseStudyMetricDto>.Failure(
                            "Case study not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    var metric = CaseStudyMetric.Create(
                        request.CaseStudyId,
                        request.MetricName,
                        request.MetricValue,
                        request.Icon,
                        displayOrder: 0
                    );

                    metric.CreatedBy = _currentUserService.UserId ?? "System";
                    metric.CreatedDate = DateTime.UtcNow;

                    await _unitOfWork.CaseStudyMetrics.AddAsync(metric);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Case study metric created: Id={MetricId}, CaseStudyId={CaseStudyId}",
                        metric.Id, request.CaseStudyId);

                    return Result<CaseStudyMetricDto>.Success(_mapper.Map<CaseStudyMetricDto>(metric));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating case study metric");
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<CaseStudyMetricDto>.Failure($"Failed to create metric: {ex.Message}");
                }
            }
        }
    }
}