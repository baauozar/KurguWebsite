// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/UpdateCaseStudyCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class UpdateCaseStudyCommand : UpdateCaseStudyDto, IRequest<Result<CaseStudyDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateCaseStudyCommandHandler
        : IRequestHandler<UpdateCaseStudyCommand, Result<CaseStudyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ICaseStudyUniquenessChecker _unique;
        private readonly ILogger<UpdateCaseStudyCommandHandler> _logger;

        public UpdateCaseStudyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ICaseStudyUniquenessChecker unique,
            ILogger<UpdateCaseStudyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _unique = unique;
            _logger = logger;
        }

        public async Task<Result<CaseStudyDto>> Handle(
            UpdateCaseStudyCommand req,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var entity = await _unitOfWork.CaseStudies.GetByIdAsync(req.Id);
                    if (entity is null)
                    {
                        return Result<CaseStudyDto>.Failure(
                            "Case study not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    var titleChanged = !string.Equals(entity.Title, req.Title, StringComparison.Ordinal);

                    if (titleChanged)
                    {
                        var baseSlug = SlugGenerator.Generate(req.Title);
                        var candidate = baseSlug;
                        var i = 2;

                        while (await _unique.SlugExistsAsync(candidate, req.Id, ct))
                        {
                            candidate = $"{baseSlug}-{i++}";
                        }

                        entity.UpdateSlug(candidate);
                    }

                    entity.Update(
                        title: req.Title,
                        clientName: req.ClientName,
                        description: req.Description,
                        challenge: req.Challenge,
                        solution: req.Solution,
                        result: req.Result
                    );

                    entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                    entity.LastModifiedDate = DateTime.UtcNow;

                    await _unitOfWork.CaseStudies.UpdateAsync(entity);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Case study updated: Id={CaseStudyId}", req.Id);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.CaseStudies,
                            CacheKeys.FeaturedCaseStudies),
                        ct);

                    return Result<CaseStudyDto>.Success(_mapper.Map<CaseStudyDto>(entity));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating case study: {CaseStudyId}", req.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<CaseStudyDto>.Failure($"Failed to update case study: {ex.Message}");
                }
            }
        }
    }
}