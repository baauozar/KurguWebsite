// src/Core/KurguWebsite.Application/Features/Pages/Commands/RestorePageCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Pages.Commands
{
    public class RestorePageCommand : IRequest<Result<PageDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestorePageCommandHandler
        : IRequestHandler<RestorePageCommand, Result<PageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RestorePageCommandHandler> _logger;

        public RestorePageCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<RestorePageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<PageDto>> Handle(
            RestorePageCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var entity = await _unitOfWork.Pages.GetByIdIncludingDeletedAsync(request.Id);
                    if (entity is null)
                    {
                        return Result<PageDto>.Failure(
                            "Page not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    if (entity.IsDeleted)
                    {
                        await _unitOfWork.Pages.RestoreAsync(entity);
                        entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                        entity.LastModifiedDate = DateTime.UtcNow;

                        await _unitOfWork.CommitAsync(ct);
                        await _unitOfWork.CommitTransactionAsync(ct);

                        _logger.LogInformation("Page restored: Id={PageId}, Title={Title}",
                            request.Id, entity.Title);

                        await _mediator.Publish(
                            new CacheInvalidationEvent(
                                CacheKeys.Pages,
                                string.Format(CacheKeys.PageBySlug, entity.Slug)),
                            ct);
                    }

                    return Result<PageDto>.Success(_mapper.Map<PageDto>(entity));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error restoring page: {PageId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<PageDto>.Failure($"Failed to restore page: {ex.Message}");
                }
            }
        }
    }
}