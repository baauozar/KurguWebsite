// src/Core/KurguWebsite.Application/Features/Services/Commands/CreateServiceCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class CreateServiceCommand : CreateServiceDto, IRequest<Result<ServiceDto>> { }

    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly IServiceUniquenessChecker _unique;
        private readonly ILogger<CreateServiceCommandHandler> _logger;

        public CreateServiceCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            IServiceUniquenessChecker unique,
            ILogger<CreateServiceCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _unique = unique;
            _logger = logger;
        }

        public async Task<Result<ServiceDto>> Handle(CreateServiceCommand req, CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    // Check title uniqueness
                    if (await _unique.TitleExistsAsync(req.Title, null, ct))
                    {
                        return Result<ServiceDto>.Failure(
                            "A service with this title already exists.",
                            ErrorCodes.DuplicateEntity);
                    }

                    // FIXED: Now includes IconClass and FullDescription parameters
                    var entity = Service.Create(
                        title: req.Title,
                        description: req.Description,
                        shortDescription: req.ShortDescription,
                        iconPath: req.IconPath,
                        category: req.Category,
                        iconClass: req.IconClass,
                        fullDescription: req.FullDescription
                    );

                    // Track creation
                    entity.CreatedBy = _currentUserService.UserId ?? "System";
                    entity.CreatedDate = DateTime.UtcNow;

                    // Set featured if requested
                    if (req.IsFeatured)
                    {
                        entity.SetFeatured(true);
                    }

                    // Set display order if provided
                    if (req.DisplayOrder > 0)
                    {
                        entity.SetDisplayOrder(req.DisplayOrder);
                    }

                    // Set SEO if provided
                    if (!string.IsNullOrEmpty(req.MetaTitle) ||
                        !string.IsNullOrEmpty(req.MetaDescription) ||
                        !string.IsNullOrEmpty(req.MetaKeywords))
                    {
                        entity.UpdateSeo(req.MetaTitle, req.MetaDescription, req.MetaKeywords);
                    }

                    // Ensure unique slug
                    var baseSlug = entity.Slug;
                    var candidate = baseSlug;
                    var i = 2;

                    while (await _unique.SlugExistsAsync(candidate, null, ct))
                    {
                        candidate = $"{baseSlug}-{i++}";
                    }

                    if (candidate != baseSlug)
                    {
                        entity.UpdateSlug(candidate);
                    }

                    // Add features if provided
                    if (req.Features is { Count: > 0 })
                    {
                        foreach (var f in req.Features)
                        {
                            entity.AddFeature(f.Title, f.Description, f.IconClass, f.DisplayOrder);
                        }
                    }

                    await _unitOfWork.Services.AddAsync(entity);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Service created: Id={ServiceId}, Title={Title}, Slug={Slug}",
                        entity.Id, entity.Title, entity.Slug);

                    // Invalidate cache
                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.Services,
                            CacheKeys.FeaturedServices,
                            CacheKeys.HomePage),
                        ct);

                    return Result<ServiceDto>.Success(
                        _mapper.Map<ServiceDto>(entity),
                        "Service created successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating service: {Title}", req.Title);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ServiceDto>.Failure($"Failed to create service: {ex.Message}");
                }
            }
        }
    }
}