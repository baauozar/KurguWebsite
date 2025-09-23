using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.Partners.Commands
{
    public class DeletePartnerCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeletePartnerCommandHandler : IRequestHandler<DeletePartnerCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public DeletePartnerCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<ControlResult> Handle(DeletePartnerCommand request, CancellationToken cancellationToken)
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(request.Id);
            if (partner == null) return ControlResult.Failure("Partner not found.");

            // Assuming a SoftDelete method exists on the entity
            partner.SoftDelete(_currentUserService.UserId ?? "System");
            await _unitOfWork.Partners.UpdateAsync(partner);

           

            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Partners, CacheKeys.ActivePartners, CacheKeys.HomePage), cancellationToken);

            return ControlResult.Success();
        }
    }
}