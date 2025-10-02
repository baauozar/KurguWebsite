// src/Core/KurguWebsite.Application/Features/ContactMessages/Queries/GetContactMessageByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Queries
{
    public class GetContactMessageByIdQuery : IRequest<Result<ContactMessageDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetContactMessageByIdQueryHandler
        : IRequestHandler<GetContactMessageByIdQuery, Result<ContactMessageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetContactMessageByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ContactMessageDto>> Handle(
            GetContactMessageByIdQuery request,
            CancellationToken ct)
        {
            var message = await _unitOfWork.ContactMessages.GetByIdAsync(request.Id);
            if (message == null)
            {
                return Result<ContactMessageDto>.Failure(
                    "Contact message not found.",
                    ErrorCodes.EntityNotFound);
            }

            return Result<ContactMessageDto>.Success(_mapper.Map<ContactMessageDto>(message));
        }
    }
}