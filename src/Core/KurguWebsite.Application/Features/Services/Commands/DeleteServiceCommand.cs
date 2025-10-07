// src/Core/KurguWebsite.Application/Features/Services/Commands/DeleteServiceCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class DeleteServiceCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteServiceCommandHandler> _logger;

        public DeleteServiceCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<DeleteServiceCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ControlResult> Handle(DeleteServiceCommand request, CancellationToken ct)
        {
            try
            {
                var service = await _unitOfWork.Services.GetByIdAsync(request.Id);
                if (service == null)
                {
                    return ControlResult.Failure("Service not found.");
                }

                service.SoftDelete(_currentUserService.UserId ?? "System");
                await _unitOfWork.Services.UpdateAsync(service);

                var remainingServices = (await _unitOfWork.Services.GetAllAsync())
                                        .Where(s => s.IsActive && s.Id != request.Id)
                                        .OrderBy(s => s.DisplayOrder)
                                        .ToList();

                remainingServices.Reorder();

                foreach (var item in remainingServices)
                {
                    await _unitOfWork.Services.UpdateAsync(item);
                }

                await _unitOfWork.CommitAsync(ct);

                _logger.LogInformation("Service soft-deleted and reordered: Id={ServiceId}", request.Id);

                await _mediator.Publish(
                    new CacheInvalidationEvent(
                        CacheKeys.Services,
                        CacheKeys.FeaturedServices,
                        CacheKeys.HomePage),
                    ct);

                return ControlResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service: {ServiceId}", request.Id);
                return ControlResult.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}