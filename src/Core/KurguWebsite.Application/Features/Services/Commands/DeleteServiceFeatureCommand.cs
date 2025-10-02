// src/Core/KurguWebsite.Application/Features/Services/Commands/DeleteServiceFeatureCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class DeleteServiceFeatureCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteServiceFeatureCommandHandler
        : IRequestHandler<DeleteServiceFeatureCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteServiceFeatureCommandHandler> _logger;

        public DeleteServiceFeatureCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteServiceFeatureCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ControlResult> Handle(
            DeleteServiceFeatureCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var feature = await _unitOfWork.ServiceFeatures.GetByIdAsync(request.Id);
                    if (feature == null)
                    {
                        return ControlResult.Failure("Service feature not found.");
                    }

                    feature.SoftDelete(_currentUserService.UserId ?? "System");
                    await _unitOfWork.ServiceFeatures.UpdateAsync(feature);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Service feature deleted: Id={FeatureId}", request.Id);

                    return ControlResult.Success();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting service feature: {FeatureId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    throw;
                }
            }
        }
    }
}