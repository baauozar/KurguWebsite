// src/Core/KurguWebsite.Application/Features/Services/Commands/RestoreServiceCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class RestoreServiceCommand : IRequest<Result<ServiceDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreServiceCommandHandler : IRequestHandler<RestoreServiceCommand, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RestoreServiceCommandHandler> _logger;

        public RestoreServiceCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<RestoreServiceCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ServiceDto>> Handle(RestoreServiceCommand request, CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var entity = await _unitOfWork.Services.GetByIdIncludingDeletedAsync(request.Id);
                    if (entity is null)
                    {
                        return Result<ServiceDto>.Failure("Service not found.", ErrorCodes.EntityNotFound);
                    }

                    if (entity.IsDeleted)
                    {
                        await _unitOfWork.Services.RestoreAsync(entity);
                        entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                        entity.LastModifiedDate = DateTime.UtcNow;

                        await _unitOfWork.CommitAsync(ct);
                        await _unitOfWork.CommitTransactionAsync(ct);

                        _logger.LogInformation("Service restored: Id={ServiceId}", request.Id);

                        await _mediator.Publish(
                            new CacheInvalidationEvent(CacheKeys.Services, CacheKeys.FeaturedServices),
                            ct);
                    }

                    return Result<ServiceDto>.Success(_mapper.Map<ServiceDto>(entity));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error restoring service: {ServiceId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ServiceDto>.Failure($"Failed to restore service: {ex.Message}");
                }
            }
        }
    }
}