// src/Core/KurguWebsite.Application/Features/Partners/Commands/DeletePartnerCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Partners.Commands
{
    public class DeletePartnerCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeletePartnerCommandHandler
        : IRequestHandler<DeletePartnerCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<DeletePartnerCommandHandler> _logger;

        public DeletePartnerCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<DeletePartnerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ControlResult> Handle(
              DeletePartnerCommand request,
              CancellationToken ct)
        {
            try
            {
                var partner = await _unitOfWork.Partners.GetByIdAsync(request.Id);
                if (partner == null)
                {
                    return ControlResult.Failure("Partner not found.");
                }

                var partnerType = partner.Type; // Store type before deleting

                // 1. Perform soft delete
                partner.SoftDelete(_currentUserService.UserId ?? "System");
                await _unitOfWork.Partners.UpdateAsync(partner);

                // 2. Get remaining active partners of the same type
                var remainingPartners = (await _unitOfWork.Partners.GetAllAsync())
                                        .Where(p => p.IsActive && p.Type == partnerType && p.Id != request.Id)
                                        .OrderBy(p => p.DisplayOrder)
                                        .ToList();

                // 3. Reorder
                remainingPartners.Reorder();

                // 4. Mark for update
                foreach (var item in remainingPartners)
                {
                    await _unitOfWork.Partners.UpdateAsync(item);
                }

                // 5. Commit all changes
                await _unitOfWork.CommitAsync(ct);

                _logger.LogInformation("Partner soft-deleted and reordered: Id={PartnerId}", request.Id);

                await _mediator.Publish(
                    new CacheInvalidationEvent(
                        CacheKeys.Partners,
                        CacheKeys.ActivePartners,
                        CacheKeys.HomePage),
                    ct);

                return ControlResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting partner: {PartnerId}", request.Id);
                return ControlResult.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}