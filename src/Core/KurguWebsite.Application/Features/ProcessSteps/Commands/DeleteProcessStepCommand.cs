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

        public DeleteProcessStepCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<ControlResult> Handle(DeleteProcessStepCommand request, CancellationToken cancellationToken)
        {
            var processStep = await _unitOfWork.ProcessSteps.GetByIdAsync(request.Id);
            if (processStep == null) return ControlResult.Failure("Process Step not found.");

            await _unitOfWork.ProcessSteps.DeleteAsync(processStep);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.ProcessSteps, CacheKeys.HomePage, CacheKeys.ServicesPage), cancellationToken);

            return ControlResult.Success();
        }
    }
}