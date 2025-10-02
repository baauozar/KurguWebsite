// src/Core/KurguWebsite.Application/Features/ContactMessages/Commands/DeleteContactMessageCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.ContactMessages.Commands
{
    public class DeleteContactMessageCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteContactMessageCommandHandler
        : IRequestHandler<DeleteContactMessageCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteContactMessageCommandHandler> _logger;

        public DeleteContactMessageCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteContactMessageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ControlResult> Handle(
            DeleteContactMessageCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var message = await _unitOfWork.ContactMessages.GetByIdAsync(request.Id);
                    if (message == null)
                    {
                        return ControlResult.Failure("Contact message not found.");
                    }

                    message.SoftDelete(_currentUserService.UserId ?? "System");
                    await _unitOfWork.ContactMessages.UpdateAsync(message);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Contact message deleted: Id={MessageId}", request.Id);

                    return ControlResult.Success();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting contact message: {MessageId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    throw;
                }
            }
        }
    }
}