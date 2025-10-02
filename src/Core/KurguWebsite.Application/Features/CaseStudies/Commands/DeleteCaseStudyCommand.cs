// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/DeleteCaseStudyCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class DeleteCaseStudyCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCaseStudyCommandHandler
        : IRequestHandler<DeleteCaseStudyCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteCaseStudyCommandHandler> _logger;

        public DeleteCaseStudyCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<DeleteCaseStudyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ControlResult> Handle(
            DeleteCaseStudyCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(request.Id);
                    if (caseStudy == null)
                    {
                        return ControlResult.Failure("Case study not found.");
                    }

                    caseStudy.SoftDelete(_currentUserService.UserId ?? "System");
                    await _unitOfWork.CaseStudies.UpdateAsync(caseStudy);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Case study deleted: Id={CaseStudyId}", request.Id);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.CaseStudies,
                            CacheKeys.FeaturedCaseStudies),
                        ct);

                    return ControlResult.Success();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting case study: {CaseStudyId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    throw;
                }
            }
        }
    }
}