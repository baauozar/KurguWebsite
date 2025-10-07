// src/Core/KurguWebsite.Application/Features/Services/Commands/DeleteServiceFeatureCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Common;
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
            try
            {
                var feature = await _unitOfWork.ServiceFeatures.GetByIdAsync(request.Id);
                if (feature == null)
                {
                    return ControlResult.Failure("Service feature not found.");
                }

                var serviceId = feature.ServiceId; // Store parent ID

                feature.SoftDelete(_currentUserService.UserId ?? "System");
                await _unitOfWork.ServiceFeatures.UpdateAsync(feature);

                var remainingFeatures = (await _unitOfWork.ServiceFeatures.GetAllAsync())
                                        .Where(f => f.IsActive && f.ServiceId == serviceId && f.Id != request.Id)
                                        .OrderBy(f => f.DisplayOrder)
                                        .ToList();

                remainingFeatures.Reorder();

                foreach (var item in remainingFeatures)
                {
                    await _unitOfWork.ServiceFeatures.UpdateAsync(item);
                }

                await _unitOfWork.CommitAsync(ct);

                _logger.LogInformation("Service feature soft-deleted and reordered: Id={FeatureId}", request.Id);
                return ControlResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service feature: {FeatureId}", request.Id);
                return ControlResult.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}