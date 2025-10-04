using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class RestoreCaseStudyCommand : IRequest<Result<CaseStudyDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreCaseStudyCommandHandler
        : IRequestHandler<RestoreCaseStudyCommand, Result<CaseStudyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RestoreCaseStudyCommandHandler> _logger;

        public RestoreCaseStudyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<RestoreCaseStudyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CaseStudyDto>> Handle(
            RestoreCaseStudyCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var entity = await _unitOfWork.CaseStudies.GetByIdIncludingDeletedAsync(request.Id);
                    if (entity is null)
                    {
                        return Result<CaseStudyDto>.Failure(
                            "Case study not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    if (entity.IsDeleted)
                    {
                        await _unitOfWork.CaseStudies.RestoreAsync(entity);
                        entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                        entity.LastModifiedDate = DateTime.UtcNow;

                        await _unitOfWork.CommitAsync(ct);
                        await _unitOfWork.CommitTransactionAsync(ct);

                        _logger.LogInformation("Case study restored: Id={CaseStudyId}", request.Id);

                        await _mediator.Publish(
                            new CacheInvalidationEvent(
                                CacheKeys.CaseStudies,
                                CacheKeys.FeaturedCaseStudies),
                            ct);
                    }

                    return Result<CaseStudyDto>.Success(_mapper.Map<CaseStudyDto>(entity));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error restoring case study: {CaseStudyId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<CaseStudyDto>.Failure($"Failed to restore case study: {ex.Message}");
                }
            }
        }
    }
}