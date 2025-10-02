// src/Core/KurguWebsite.Application/Features/ContactMessages/Commands/ReplyToContactMessageCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.ContactMessages.Commands
{
    public class ReplyToContactMessageCommand : IRequest<Result<ContactMessageDto>>
    {
        public Guid Id { get; set; }
        public string ReplyMessage { get; set; } = string.Empty;
    }

    public class ReplyToContactMessageCommandHandler
        : IRequestHandler<ReplyToContactMessageCommand, Result<ContactMessageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ReplyToContactMessageCommandHandler> _logger;

        public ReplyToContactMessageCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<ReplyToContactMessageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ContactMessageDto>> Handle(
            ReplyToContactMessageCommand request,
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

                    message.MarkAsReplied(_currentUserService.UserId ?? "System");

                    await _unitOfWork.ContactMessages.UpdateAsync(message);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Reply sent to contact message: Id={MessageId}", request.Id);

                    return Result<ContactMessageDto>.Success(_mapper.Map<ContactMessageDto>(message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error replying to message: {MessageId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ContactMessageDto>.Failure($"Failed to reply to message: {ex.Message}");
                }
            }
        }
    }
}