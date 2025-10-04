// src/Core/KurguWebsite.Application/Features/Services/Commands/RestoreServiceFeatureCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class RestoreServiceFeatureCommand : IRequest<Result<ServiceFeatureDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreServiceFeatureCommandHandler
        : IRequestHandler<RestoreServiceFeatureCommand, Result<ServiceFeatureDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RestoreServiceFeatureCommandHandler> _logger;

        public RestoreServiceFeatureCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<RestoreServiceFeatureCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ServiceFeatureDto>> Handle(
            RestoreServiceFeatureCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var entity = await _unitOfWork.ServiceFeatures.GetByIdIncludingDeletedAsync(request.Id);
                    if (entity is null)
                    {
                        return Result<ServiceFeatureDto>.Failure(
                            "Service feature not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    if (entity.IsDeleted)
                    {
                        await _unitOfWork.ServiceFeatures.RestoreAsync(entity);
                        entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                        entity.LastModifiedDate = DateTime.UtcNow;

                        await _unitOfWork.CommitAsync(ct);
                        await _unitOfWork.CommitTransactionAsync(ct);

                        _logger.LogInformation("Service feature restored: Id={FeatureId}, ServiceId={ServiceId}",
                            request.Id, entity.ServiceId);

                        // Invalidate service caches
                        await _mediator.Publish(
                            new CacheInvalidationEvent(
                                CacheKeys.Services,
                                CacheKeys.FeaturedServices,
                                string.Format(CacheKeys.ServiceById, entity.ServiceId)),
                            ct);
                    }

                    return Result<ServiceFeatureDto>.Success(_mapper.Map<ServiceFeatureDto>(entity));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error restoring service feature: {FeatureId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ServiceFeatureDto>.Failure($"Failed to restore service feature: {ex.Message}");
                }
            }
        }
    }
}