using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Commands
{
    // FIX: Correct class name
    public class RestoreServiceCommand : IRequest<Result<ServiceDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreServiceCommandHandler
        : IRequestHandler<RestoreServiceCommand, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public RestoreServiceCommandHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator, ICurrentUserService currentUserService)
        {
            _uow = uow;
            _mapper = mapper;
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ServiceDto>> Handle(RestoreServiceCommand request, CancellationToken ct)
        {
            var entity = await _uow.Services.GetByIdIncludingDeletedAsync(request.Id);
            if (entity is null) return Result<ServiceDto>.Failure("Service not found.");

            if (entity.IsDeleted)
            {
                await _uow.Services.RestoreAsync(entity);

                // Track who restored
                entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                entity.LastModifiedDate = DateTime.UtcNow;

                await _uow.CommitAsync(ct);

                await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Services, CacheKeys.FeaturedServices), ct);
            }

            return Result<ServiceDto>.Success(_mapper.Map<ServiceDto>(entity));
        }
    }
}