// src/Core/KurguWebsite.Application/Features/Pages/Commands/UpdatePageContentCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Pages.Commands
{
    public class UpdatePageContentCommand : UpdatePageDto, IRequest<Result<PageDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePageContentCommandHandler
        : IRequestHandler<UpdatePageContentCommand, Result<PageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly IPageUniquenessChecker _unique;
        private readonly ILogger<UpdatePageContentCommandHandler> _logger;

        public UpdatePageContentCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            IPageUniquenessChecker unique,
            ILogger<UpdatePageContentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _unique = unique;
            _logger = logger;
        }

        public async Task<Result<PageDto>> Handle(
            UpdatePageContentCommand req,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var page = await _unitOfWork.Pages.GetByIdAsync(req.Id);
                    if (page is null)
                    {
                        return Result<PageDto>.Failure(
                            "Page not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    var titleChanged = !string.Equals(page.Title, req.Title, StringComparison.Ordinal);

                    if (titleChanged)
                    {
                        var baseSlug = SlugGenerator.Generate(req.Title);
                        var candidate = baseSlug;
                        var i = 2;

                        while (await _unique.PageSlugExistsAsync(candidate, req.Id, ct))
                        {
                            candidate = $"{baseSlug}-{i++}";
                        }

                        page.UpdateSlug(candidate);
                    }

                    page.Update(
                          req.Title ?? page.Title,
                          req.PageType ?? page.PageType,
                          req.Content ?? page.Content
                      );

                    page.UpdateSeo(
                        req.MetaTitle,
                        req.MetaDescription,
                        req.MetaKeywords
                    );

                    page.LastModifiedBy = _currentUserService.UserId ?? "System";
                    page.LastModifiedDate = DateTime.UtcNow;

                    await _unitOfWork.Pages.UpdateAsync(page);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Page updated: Id={PageId}, Title={Title}", req.Id, page.Title);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.Pages,
                            string.Format(CacheKeys.PageBySlug, page.Slug)),
                        ct);

                    return Result<PageDto>.Success(_mapper.Map<PageDto>(page));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating page: {PageId}", req.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<PageDto>.Failure($"Failed to update page: {ex.Message}");
                }
            }
        }
    }
}