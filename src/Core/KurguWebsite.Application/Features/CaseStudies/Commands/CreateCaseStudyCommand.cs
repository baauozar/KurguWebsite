// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/CreateCaseStudyCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class CreateCaseStudyCommand : CreateCaseStudyDto, IRequest<Result<CaseStudyDto>> { }

    public class CreateCaseStudyCommandHandler
        : IRequestHandler<CreateCaseStudyCommand, Result<CaseStudyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ICaseStudyUniquenessChecker _unique;
        private readonly ILogger<CreateCaseStudyCommandHandler> _logger;

        public CreateCaseStudyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ICaseStudyUniquenessChecker unique,
            ILogger<CreateCaseStudyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _unique = unique;
            _logger = logger;
        }

        public async Task<Result<CaseStudyDto>> Handle(
            CreateCaseStudyCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    // FIXED: Now includes Industry parameter
                    var entity = CaseStudy.Create(
                        title: request.Title,
                        clientName: request.ClientName,
                        description: request.Description,
                        imagePath: request.ImagePath,
                        completedDate: request.CompletedDate,
                        industry: request.Industry
                    );

                    entity.CreatedBy = _currentUserService.UserId ?? "System";
                    entity.CreatedDate = DateTime.UtcNow;

                    // Set ServiceId if provided
                    if (request.ServiceId.HasValue)
                    {
                        entity.SetService(request.ServiceId.Value);
                    }

                    // Set IsFeatured if provided
                    if (request.IsFeatured)
                    {
                        entity.SetFeatured(true);
                    }

                    // Add technologies if provided
                    if (request.Technologies is { Count: > 0 })
                    {
                        foreach (var tech in request.Technologies)
                        {
                            entity.AddTechnology(tech);
                        }
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

                    await _unitOfWork.CaseStudies.AddAsync(entity);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Case study created: Id={CaseStudyId}, Title={Title}, Slug={Slug}",
                        entity.Id, entity.Title, entity.Slug);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.CaseStudies,
                            CacheKeys.FeaturedCaseStudies),
                        ct);

                    return Result<CaseStudyDto>.Success(_mapper.Map<CaseStudyDto>(entity));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating case study: {Title}", request.Title);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<CaseStudyDto>.Failure($"Failed to create case study: {ex.Message}");
                }
            }
        }
    }
}