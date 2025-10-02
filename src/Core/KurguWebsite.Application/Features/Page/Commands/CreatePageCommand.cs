// src/Core/KurguWebsite.Application/Features/Pages/Commands/CreatePageCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Pages.Commands
{
    public class CreatePageCommand : CreatePageDto, IRequest<Result<PageDto>> { }

    public class CreatePageCommandHandler
        : IRequestHandler<CreatePageCommand, Result<PageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly IPageUniquenessChecker _unique;
        private readonly ILogger<CreatePageCommandHandler> _logger;

        public CreatePageCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            IPageUniquenessChecker unique,
            ILogger<CreatePageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _unique = unique;
            _logger = logger;
        }

        public async Task<Result<PageDto>> Handle(
            CreatePageCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var page = Page.Create(request.Title, request.PageType);

                    page.CreatedBy = _currentUserService.UserId ?? "System";
                    page.CreatedDate = DateTime.UtcNow;

                    // Ensure unique slug
                    var baseSlug = page.Slug;
                    var candidate = baseSlug;
                    var i = 2;

                    while (await _unique.PageSlugExistsAsync(candidate, null, ct))
                    {
                        candidate = $"{baseSlug}-{i++}";
                    }

                    if (candidate != baseSlug)
                    {
                        page.UpdateSlug(candidate);
                    }

                    await _unitOfWork.Pages.AddAsync(page);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Page created: Id={PageId}, Title={Title}, Slug={Slug}",
                        page.Id, page.Title, page.Slug);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(CacheKeys.Pages),
                        ct);

                    return Result<PageDto>.Success(_mapper.Map<PageDto>(page));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating page: {Title}", request.Title);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<PageDto>.Failure($"Failed to create page: {ex.Message}");
                }
            }
        }
    }
}