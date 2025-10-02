using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.ProcessSteps.Commands
{
    public class DeleteProcessStepCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteProcessStepCommandHandler : IRequestHandler<DeleteProcessStepCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProcessStepCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public async Task<ControlResult> Handle(DeleteProcessStepCommand request, CancellationToken cancellationToken)
        {
            var processStep = await _unitOfWork.ProcessSteps.GetByIdAsync(request.Id);
            if (processStep == null) return ControlResult.Failure("Process Step not found.");

            // FIX: Use soft delete instead of hard delete
            processStep.SoftDelete(_currentUserService.UserId ?? "System");
            await _unitOfWork.ProcessSteps.UpdateAsync(processStep);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.ProcessSteps, CacheKeys.HomePage, CacheKeys.ServicesPage), cancellationToken);

            return ControlResult.Success();
        }
    }
}