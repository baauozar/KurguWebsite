using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using KurguWebsite.Domain.Entities;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Commands
{
    public class SubmitContactMessageCommand : CreateContactMessageDto, IRequest<Result<ContactMessageDto>> { }

    public class SubmitContactMessageCommandHandler : IRequestHandler<SubmitContactMessageCommand, Result<ContactMessageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubmitContactMessageCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ContactMessageDto>> Handle(SubmitContactMessageCommand request, CancellationToken cancellationToken)
        {
            var message = ContactMessage.Create(request.Name, request.Email, request.Subject, request.Message,request.Phone);
            // The ContactMessageReceivedEvent is already added in the entity's Create method

            await _unitOfWork.ContactMessages.AddAsync(message);
            await _unitOfWork.CommitAsync();

            return Result<ContactMessageDto>.Success(_mapper.Map<ContactMessageDto>(message));
        }
    }
}