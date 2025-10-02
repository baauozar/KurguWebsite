using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Commands
{
    // FIX: Correct class name
    public class RestoreContactMessageCommand : IRequest<Result<ContactMessageDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreContactMessageCommandHandler
        : IRequestHandler<RestoreContactMessageCommand, Result<ContactMessageDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public RestoreContactMessageCommandHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator, ICurrentUserService currentUserService)
        {
            _uow = uow;
            _mapper = mapper;
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ContactMessageDto>> Handle(RestoreContactMessageCommand request, CancellationToken ct)
        {
            var entity = await _uow.ContactMessages.GetByIdIncludingDeletedAsync(request.Id);
            if (entity is null) return Result<ContactMessageDto>.Failure("Contact message not found.");

            if (entity.IsDeleted)
            {
                await _uow.ContactMessages.RestoreAsync(entity);

                // Track who restored
                entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                entity.LastModifiedDate = DateTime.UtcNow;

                await _uow.CommitAsync(ct);
            }

            return Result<ContactMessageDto>.Success(_mapper.Map<ContactMessageDto>(entity));
        }
    }
}