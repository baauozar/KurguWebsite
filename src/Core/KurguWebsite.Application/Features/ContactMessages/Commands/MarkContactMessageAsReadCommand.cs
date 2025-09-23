using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Commands
{
    public class MarkContactMessageAsReadCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class MarkContactMessageAsReadCommandHandler : IRequestHandler<MarkContactMessageAsReadCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public MarkContactMessageAsReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ControlResult> Handle(MarkContactMessageAsReadCommand request, CancellationToken cancellationToken)
        {
            var message = await _unitOfWork.ContactMessages.GetByIdAsync(request.Id);
            if (message == null) return ControlResult.Failure("Contact message not found.");

            message.MarkAsRead();
            await _unitOfWork.ContactMessages.UpdateAsync(message);
            await _unitOfWork.CommitAsync(cancellationToken);

            return ControlResult.Success();
        }
    }
}