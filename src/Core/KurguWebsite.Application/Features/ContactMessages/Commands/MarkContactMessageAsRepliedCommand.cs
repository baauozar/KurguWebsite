using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.ContactMessages.Commands
{
    public class MarkContactMessageAsRepliedCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class MarkContactMessageAsRepliedCommandHandler : IRequestHandler<MarkContactMessageAsRepliedCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public MarkContactMessageAsRepliedCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ControlResult> Handle(MarkContactMessageAsRepliedCommand request, CancellationToken cancellationToken)
        {
            var message = await _unitOfWork.ContactMessages.GetByIdAsync(request.Id);
            if (message == null)
            {
                return ControlResult.Failure("Contact message not found.");
            }

            var userId = _currentUserService.UserId ?? "System";
            message.MarkAsReplied(userId);

            // FIX: Track who modified
            message.LastModifiedBy = userId;
            message.LastModifiedDate = DateTime.UtcNow;

            await _unitOfWork.ContactMessages.UpdateAsync(message);
            await _unitOfWork.CommitAsync(cancellationToken);

            return ControlResult.Success();
        }
    }
}