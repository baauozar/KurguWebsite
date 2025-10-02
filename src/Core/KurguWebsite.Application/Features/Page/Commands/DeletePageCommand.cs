// src/Core/KurguWebsite.Application/Features/Pages/Commands/DeletePageCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Pages.Commands
{
    public class DeletePageCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeletePageCommandHandler
        : IRequestHandler<DeletePageCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<DeletePageCommandHandler> _logger;

        public DeletePageCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<DeletePageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ControlResult> Handle(
            DeletePageCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var page = await _unitOfWork.Pages.GetByIdAsync(request.Id);
                    if (page == null)
                    {
                        return ControlResult.Failure("Page not found.");
                    }

                    page.SoftDelete(_currentUserService.UserId ?? "System");
                    await _unitOfWork.Pages.UpdateAsync(page);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Page deleted: Id={PageId}", request.Id);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(CacheKeys.Pages),
                        ct);

                    return ControlResult.Success();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting page: {PageId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    throw;
                }
            }
        }
    }
}