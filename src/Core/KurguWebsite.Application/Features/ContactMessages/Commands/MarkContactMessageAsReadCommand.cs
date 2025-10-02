// src/Core/KurguWebsite.Application/Features/ContactMessages/Commands/MarkContactMessageAsReadCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.ContactMessages.Commands
{
    public class MarkContactMessageAsReadCommand : IRequest<Result<ContactMessageDto>>
    {
        public Guid Id { get; set; }
    }

    public class MarkContactMessageAsReadCommandHandler
        : IRequestHandler<MarkContactMessageAsReadCommand, Result<ContactMessageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<MarkContactMessageAsReadCommandHandler> _logger;

        public MarkContactMessageAsReadCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<MarkContactMessageAsReadCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ContactMessageDto>> Handle(
            MarkContactMessageAsReadCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var message = await _unitOfWork.ContactMessages.GetByIdAsync(request.Id);
                    if (message is null)
                    {
                        return Result<ContactMessageDto>.Failure(
                            "Contact message not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    message.MarkAsRead();

                    await _unitOfWork.ContactMessages.UpdateAsync(message);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Contact message marked as read: Id={MessageId}", request.Id);

                    return Result<ContactMessageDto>.Success(_mapper.Map<ContactMessageDto>(message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error marking message as read: {MessageId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ContactMessageDto>.Failure($"Failed to mark message as read: {ex.Message}");
                }
            }
        }
    }
}