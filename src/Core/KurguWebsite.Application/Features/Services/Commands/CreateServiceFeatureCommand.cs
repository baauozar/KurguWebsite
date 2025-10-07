using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq; // Ensure you have this using statement

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class CreateServiceFeatureCommand : IRequest<Result<ServiceFeatureDto>>
    {
        // These are the only properties the client should provide
        public Guid ServiceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        // DisplayOrder is removed from here
    }

    public class CreateServiceFeatureCommandHandler
        : IRequestHandler<CreateServiceFeatureCommand, Result<ServiceFeatureDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateServiceFeatureCommandHandler> _logger;

        public CreateServiceFeatureCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<CreateServiceFeatureCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ServiceFeatureDto>> Handle(
            CreateServiceFeatureCommand request,
            CancellationToken ct)
        {
            try
            {
                var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId);
                if (service == null)
                {
                    return Result<ServiceFeatureDto>.Failure("Service not found.", ErrorCodes.EntityNotFound);
                }

                // ✅ CORRECTED: Get all features for THIS service to calculate the next DisplayOrder
                var featuresForService = await _unitOfWork.ServiceFeatures.GetAsync(f => f.ServiceId == request.ServiceId, cancellationToken: ct);
                var maxOrder = featuresForService.Any() ? featuresForService.Max(f => f.DisplayOrder) : 0;
                var nextDisplayOrder = maxOrder + 1;

                // Create the entity without passing DisplayOrder
                var entity = ServiceFeature.Create(
                    request.ServiceId,
                    request.Title,
                    request.Description,
                    request.IconClass
                );

                entity.CreatedBy = _currentUserService.UserId ?? "System";
                entity.CreatedDate = DateTime.UtcNow;

                // Set the calculated display order
                entity.SetDisplayOrder(nextDisplayOrder);

                await _unitOfWork.ServiceFeatures.AddAsync(entity);
                await _unitOfWork.CommitAsync(ct); // Single commit for the transaction

                _logger.LogInformation(
                    "Service feature created: Id={FeatureId}, ServiceId={ServiceId}",
                    entity.Id, request.ServiceId);

                var dto = _mapper.Map<ServiceFeatureDto>(entity);
                return Result<ServiceFeatureDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service feature for ServiceId {ServiceId}", request.ServiceId);
                // No manual rollback needed, UnitOfWork handles it
                return Result<ServiceFeatureDto>.Failure($"Failed to create feature: {ex.Message}");
            }
        }
    }
}