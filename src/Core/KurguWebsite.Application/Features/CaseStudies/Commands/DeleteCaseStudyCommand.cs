// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/DeleteCaseStudyCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Common;
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
           try
            {
                var caseStudyToDelete = await _unitOfWork.CaseStudies.GetByIdAsync(request.Id);
                if (caseStudyToDelete == null)
                {
                    return ControlResult.Failure("Case study not found.");
                }

                // 1. Perform the soft delete on the entity
                caseStudyToDelete.SoftDelete(_currentUserService.UserId ?? "System");
                await _unitOfWork.CaseStudies.UpdateAsync(caseStudyToDelete);

                // 2. Get all *other* active case studies that need reordering
                var activeCaseStudies = (await _unitOfWork.CaseStudies.GetAllAsync())
                                        .Where(cs => cs.IsActive && cs.Id != request.Id) // Exclude the one being deleted
                                        .OrderBy(cs => cs.DisplayOrder)
                                        .ToList();

                // 3. Reorder the list in memory
                // This requires the OrderableExtensions class to be in your project.
                activeCaseStudies.Reorder();

                // 4. Update the reordered entities in the context
                // The UnitOfWork will track these changes.
                foreach (var caseStudy in activeCaseStudies)
                {
                    await _unitOfWork.CaseStudies.UpdateAsync(caseStudy);
                }

                // 5. Commit all changes (soft delete + reordering) in one transaction
                await _unitOfWork.CommitAsync(ct);

                _logger.LogInformation("Case study soft-deleted and active studies reordered: Id={CaseStudyId}", request.Id);

                // Invalidate cache after successful commit
                await _mediator.Publish(
                    new CacheInvalidationEvent(
                        CacheKeys.CaseStudies,
                        CacheKeys.FeaturedCaseStudies),
                    ct);

                return ControlResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during soft-delete and reorder process for case study: {CaseStudyId}", request.Id);
                // No need to manually rollback, the UnitOfWork or middleware should handle it
                return ControlResult.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}