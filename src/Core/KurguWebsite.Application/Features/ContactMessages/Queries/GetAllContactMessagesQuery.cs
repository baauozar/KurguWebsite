// src/Core/KurguWebsite.Application/Features/ContactMessages/Queries/GetAllContactMessagesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Queries
{
    public class GetAllContactMessagesQuery : IRequest<Result<List<ContactMessageDto>>>
    {
        public bool IncludeRead { get; set; } = true;
    }

    public class GetAllContactMessagesQueryHandler
        : IRequestHandler<GetAllContactMessagesQuery, Result<List<ContactMessageDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllContactMessagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ContactMessageDto>>> Handle(
            GetAllContactMessagesQuery request,
            CancellationToken ct)
        {
            var messages = request.IncludeRead
                ? await _unitOfWork.ContactMessages.GetAllAsync()
                : await _unitOfWork.ContactMessages.GetUnreadMessagesAsync();

            var mappedMessages = _mapper.Map<List<ContactMessageDto>>(messages);
            return Result<List<ContactMessageDto>>.Success(mappedMessages);
        }
    }
}