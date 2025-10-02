// src/Core/KurguWebsite.Application/Features/Services/Commands/UpdateServiceCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class UpdateServiceCommand : UpdateServiceDto, IRequest<Result<ServiceDto>>
    {
        public new Guid Id { get; set; }
    }

    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly IServiceUniquenessChecker _unique;
        private readonly ILogger<UpdateServiceCommandHandler> _logger;

        public UpdateServiceCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            IServiceUniquenessChecker unique,
            ILogger<UpdateServiceCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _unique = unique;
            _logger = logger;
        }

        public async Task<Result<ServiceDto>> Handle(UpdateServiceCommand req, CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var entity = await _unitOfWork.Services.GetByIdAsync(req.Id);
                    if (entity is null)
                    {
                        return Result<ServiceDto>.Failure(
                            "Service not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    var titleChanged = !string.Equals(entity.Title, req.Title, StringComparison.Ordinal);

                    if (titleChanged)
                    {
                        // Check title uniqueness
                        if (await _unique.TitleExistsAsync(req.Title, req.Id, ct))
                        {
                            return Result<ServiceDto>.Failure(
                                "A service with this title already exists.",
                                ErrorCodes.DuplicateEntity);
                        }

                        // Generate new slug
                        var baseSlug = SlugGenerator.Generate(req.Title);
                        var candidate = baseSlug;
                        var i = 2;

                        while (await _unique.SlugExistsAsync(candidate, req.Id, ct))
                        {
                            candidate = $"{baseSlug}-{i++}";
                        }

                        entity.UpdateSlug(candidate);
                    }

                    // Update entity
                    entity.Update(
                        title: req.Title,
                        description: req.Description,
                        shortDescription: req.ShortDescription,
                        fullDescription: req.FullDescription,
                        iconPath: req.IconPath,
                        category: req.Category
                    );

                    entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                    entity.LastModifiedDate = DateTime.UtcNow;

                    await _unitOfWork.Services.UpdateAsync(entity);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Service updated: Id={ServiceId}, Title={Title}",
                        entity.Id, entity.Title);

                    // Invalidate cache
                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.Services,
                            CacheKeys.FeaturedServices,
                            string.Format(CacheKeys.ServiceById, req.Id),
                            string.Format(CacheKeys.ServiceBySlug, entity.Slug),
                            CacheKeys.HomePage),
                        ct);

                    return Result<ServiceDto>.Success(
                        _mapper.Map<ServiceDto>(entity),
                        "Service updated successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating service: {ServiceId}", req.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ServiceDto>.Failure($"Failed to update service: {ex.Message}");
                }
            }
        }
    }
}