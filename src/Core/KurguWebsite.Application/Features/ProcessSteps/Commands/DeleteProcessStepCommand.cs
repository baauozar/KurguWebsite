// src/Core/KurguWebsite.Application/Features/ProcessSteps/Commands/DeleteProcessStepCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.ProcessSteps.Commands
{
    public class DeleteProcessStepCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteProcessStepCommandHandler
        : IRequestHandler<DeleteProcessStepCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteProcessStepCommandHandler> _logger;

        public DeleteProcessStepCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<DeleteProcessStepCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ControlResult> Handle(
            DeleteProcessStepCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var processStep = await _unitOfWork.ProcessSteps.GetByIdAsync(request.Id);
                    if (processStep == null)
                    {
                        return ControlResult.Failure("Process step not found.");
                    }

                    processStep.SoftDelete(_currentUserService.UserId ?? "System");
                    await _unitOfWork.ProcessSteps.UpdateAsync(processStep);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Process step deleted: Id={ProcessStepId}", request.Id);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.ProcessSteps,
                            CacheKeys.HomePage),
                        ct);

                    return ControlResult.Success();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting process step: {ProcessStepId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    throw;
                }
            }
        }
    }
}