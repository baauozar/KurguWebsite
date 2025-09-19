using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Services.Concrete
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ContactMessageService> _logger;

        public ContactMessageService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            ICurrentUserService currentUserService,
            ILogger<ContactMessageService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ContactMessageDto>> SubmitMessageAsync(CreateContactMessageDto dto)
        {
            try
            {
                var message = ContactMessage.Create(
                    dto.Name,
                    dto.Email,
                    dto.Phone,
                    dto.Subject,
                    dto.Message);

                message.SetCreatedBy(dto.Email);

                await _unitOfWork.ContactMessages.AddAsync(message);
                await _unitOfWork.CommitAsync();

                // Send email notification to admin
                try
                {
                    await _emailService.SendContactFormEmailAsync(
                        dto.Name,
                        dto.Email,
                        dto.Subject,
                        dto.Message);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send contact form email notification");
                    // Don't fail the request if email fails
                }

                var messageDto = _mapper.Map<ContactMessageDto>(message);
                return Result<ContactMessageDto>.Success(messageDto, "Your message has been sent successfully. We'll get back to you soon!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting contact message");
                return Result<ContactMessageDto>.Failure($"An error occurred while sending your message. Please try again.");
            }
        }

        public async Task<Result<PaginatedList<ContactMessageDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var messages = await _unitOfWork.ContactMessages.GetAsync(
    predicate: null,
    orderBy: q => q.OrderByDescending(m => m.CreatedDate),
    includeString: null,
    disableTracking: true);
                var totalCount = messages.Count;
                var items = messages
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var messageDtos = _mapper.Map<List<ContactMessageDto>>(items);
                var paginatedList = new PaginatedList<ContactMessageDto>(messageDtos, totalCount, pageNumber, pageSize);

                return Result<PaginatedList<ContactMessageDto>>.Success(paginatedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated messages");
                return Result<PaginatedList<ContactMessageDto>>.Failure($"An error occurred while retrieving messages");
            }
        }

        public async Task<Result<IEnumerable<ContactMessageDto>>> GetUnreadAsync()
        {
            try
            {
                var messages = await _unitOfWork.ContactMessages.GetUnreadMessagesAsync();
                var messageDtos = _mapper.Map<IEnumerable<ContactMessageDto>>(messages);

                return Result<IEnumerable<ContactMessageDto>>.Success(messageDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread messages");
                return Result<IEnumerable<ContactMessageDto>>.Failure($"An error occurred while retrieving messages");
            }
        }

        public async Task<Result<ContactMessageDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);

                if (message == null)
                    return Result<ContactMessageDto>.Failure($"Message with id {id} not found");

                // Mark as read when retrieved
                if (!message.IsRead)
                {
                    message.MarkAsRead();
                    await _unitOfWork.ContactMessages.UpdateAsync(message);
                    await _unitOfWork.CommitAsync();
                }

                var messageDto = _mapper.Map<ContactMessageDto>(message);
                return Result<ContactMessageDto>.Success(messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving message by id {MessageId}", id);
                return Result<ContactMessageDto>.Failure($"An error occurred while retrieving the message");
            }
        }

        public async Task<Result<bool>> MarkAsReadAsync(Guid id)
        {
            try
            {
                var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);

                if (message == null)
                    return Result<bool>.Failure($"Message with id {id} not found");

                if (!message.IsRead)
                {
                    message.MarkAsRead();
                    await _unitOfWork.ContactMessages.UpdateAsync(message);
                    await _unitOfWork.CommitAsync();
                }

                return Result<bool>.Success(true, "Message marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read {MessageId}", id);
                return Result<bool>.Failure($"An error occurred while updating the message");
            }
        }

        public async Task<Result<bool>> MarkAsRepliedAsync(Guid id, string userId)
        {
            try
            {
                var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);

                if (message == null)
                    return Result<bool>.Failure($"Message with id {id} not found");

                if (!message.IsReplied)
                {
                    message.MarkAsReplied(userId);
                    await _unitOfWork.ContactMessages.UpdateAsync(message);
                    await _unitOfWork.CommitAsync();
                }

                return Result<bool>.Success(true, "Message marked as replied");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as replied {MessageId}", id);
                return Result<bool>.Failure($"An error occurred while updating the message");
            }
        }

        public async Task<Result<int>> GetUnreadCountAsync()
        {
            try
            {
                var count = await _unitOfWork.ContactMessages.GetUnreadCountAsync();
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread message count");
                return Result<int>.Failure($"An error occurred while retrieving the count");
            }
        }
    }
}