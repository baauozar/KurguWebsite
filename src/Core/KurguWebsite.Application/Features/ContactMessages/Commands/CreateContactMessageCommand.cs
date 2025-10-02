// src/Core/KurguWebsite.Application/Features/ContactMessages/Commands/CreateContactMessageCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using KurguWebsite.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.ContactMessages.Commands
{
    public class CreateContactMessageCommand : CreateContactMessageDto,
        IRequest<Result<ContactMessageDto>>
    { }

    public class CreateContactMessageCommandHandler
        : IRequestHandler<CreateContactMessageCommand, Result<ContactMessageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateContactMessageCommandHandler> _logger;

        public CreateContactMessageCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateContactMessageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ContactMessageDto>> Handle(
            CreateContactMessageCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var message = ContactMessage.Create(
                        request.Name,
                        request.Email,
                        request.Phone,
                        request.Subject,
                        request.Message
                    );

                    message.CreatedDate = DateTime.UtcNow;

                    await _unitOfWork.ContactMessages.AddAsync(message);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Contact message created: Id={MessageId}, From={Email}",
                        message.Id, message.Email);

                    return Result<ContactMessageDto>.Success(_mapper.Map<ContactMessageDto>(message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating contact message from: {Email}", request.Email);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ContactMessageDto>.Failure($"Failed to create contact message: {ex.Message}");
                }
            }
        }
    }
}