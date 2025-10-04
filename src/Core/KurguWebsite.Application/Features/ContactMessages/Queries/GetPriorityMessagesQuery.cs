// src/Core/KurguWebsite.Application/Features/ContactMessages/Queries/GetPriorityMessagesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Queries
{
    public class GetPriorityMessagesQuery : IRequest<Result<List<ContactMessageDto>>> { }

    public class GetPriorityMessagesQueryHandler
        : IRequestHandler<GetPriorityMessagesQuery, Result<List<ContactMessageDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPriorityMessagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ContactMessageDto>>> Handle(
            GetPriorityMessagesQuery request,
            CancellationToken ct)
        {
            var spec = new PriorityMessagesSpecification();
            var messages = await _unitOfWork.ContactMessages.ListAsync(spec, ct);
            var mappedMessages = _mapper.Map<List<ContactMessageDto>>(messages);

            return Result<List<ContactMessageDto>>.Success(mappedMessages);
        }
    }
}