using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Commands
{
    public class RestoreContactMessagesCommand : IRequest<Result<ContactMessageDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreContactMessagesCommandHandler
        : IRequestHandler<RestoreContactMessagesCommand, Result<ContactMessageDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public RestoreContactMessagesCommandHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator)
        {
            _uow = uow; _mapper = mapper; _mediator = mediator;
        }

        public async Task<Result<ContactMessageDto>> Handle(RestoreContactMessagesCommand request, CancellationToken ct)
        {
            // Load INCLUDING soft-deleted row
            var entity = await _uow.ContactMessages.GetByIdIncludingDeletedAsync(request.Id);
            if (entity is null) return Result<ContactMessageDto>.Failure("Process not found.");

            if (entity.IsDeleted)
            {
                // Soft restore
                await _uow.ContactMessages.RestoreAsync(entity);
                await _uow.CommitAsync(ct);

                // Invalidate caches as you do elsewhere
                await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Services, CacheKeys.FeaturedServices), ct);
            }

            return Result<ContactMessageDto>.Success(_mapper.Map<ContactMessageDto>(entity));
        }
    }
}