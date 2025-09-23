
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Events;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class DeleteServiceCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public DeleteServiceCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<ControlResult> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(request.Id);

            if (service == null)
            {
                return ControlResult.Failure("Service not found.");
            }

            // If you are using soft delete, you would call this:
            service.SoftDelete(_currentUserService.UserId ?? "System");
            await _unitOfWork.Services.UpdateAsync(service);

           

            await _unitOfWork.CommitAsync(cancellationToken);

            // Invalidate the cache after the deletion is successful
            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Services, CacheKeys.FeaturedServices, CacheKeys.HomePage), cancellationToken);

            return ControlResult.Success();
        }
    }
}