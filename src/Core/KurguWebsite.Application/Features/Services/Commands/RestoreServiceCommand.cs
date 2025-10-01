using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class RestoreProcessStepsCommand : IRequest<Result<ServiceDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreServiceCommandHandler
        : IRequestHandler<RestoreProcessStepsCommand, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public RestoreServiceCommandHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator)
        {
            _uow = uow; _mapper = mapper; _mediator = mediator;
        }

        public async Task<Result<ServiceDto>> Handle(RestoreProcessStepsCommand request, CancellationToken ct)
        {
      
            var entity = await _uow.Services.GetByIdIncludingDeletedAsync(request.Id);
            if (entity is null) return Result<ServiceDto>.Failure("Service not found.");

            if (entity.IsDeleted)
            {
                // Soft restore
                await _uow.Services.RestoreAsync(entity);
                await _uow.CommitAsync(ct);

                // Invalidate caches as you do elsewhere
                await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Services, CacheKeys.FeaturedServices), ct);
            }

            return Result<ServiceDto>.Success(_mapper.Map<ServiceDto>(entity));
        }
    }
}