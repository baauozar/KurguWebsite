// src/Core/KurguWebsite.Application/Features/Partners/Commands/DeletePartnerCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
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
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var partner = await _unitOfWork.Partners.GetByIdAsync(request.Id);
                    if (partner == null)
                    {
                        return ControlResult.Failure("Partner not found.");
                    }

                    partner.SoftDelete(_currentUserService.UserId ?? "System");
                    await _unitOfWork.Partners.UpdateAsync(partner);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Partner deleted: Id={PartnerId}", request.Id);

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
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    throw;
                }
            }
        }
    }
}