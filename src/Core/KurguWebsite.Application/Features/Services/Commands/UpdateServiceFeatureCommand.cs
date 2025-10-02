// src/Core/KurguWebsite.Application/Features/Services/Commands/UpdateServiceFeatureCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class UpdateServiceFeatureCommand : IRequest<Result<ServiceFeatureDto>>
    {
        public Guid Id { get; set; }
        public UpdateServiceFeatureDto UpdateServiceFeatureDto { get; set; } = null!;
    }

    public class UpdateServiceFeatureCommandHandler
        : IRequestHandler<UpdateServiceFeatureCommand, Result<ServiceFeatureDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateServiceFeatureCommandHandler> _logger;

        public UpdateServiceFeatureCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<UpdateServiceFeatureCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ServiceFeatureDto>> Handle(
            UpdateServiceFeatureCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var feature = await _unitOfWork.ServiceFeatures.GetByIdAsync(request.Id);
                    if (feature == null)
                    {
                        return Result<ServiceFeatureDto>.Failure(
                            "Service feature not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    feature.Update(
                        request.UpdateServiceFeatureDto.Title,
                        request.UpdateServiceFeatureDto.Description,
                        request.UpdateServiceFeatureDto.IconClass);

                    feature.LastModifiedBy = _currentUserService.UserId ?? "System";
                    feature.LastModifiedDate = DateTime.UtcNow;

                    await _unitOfWork.ServiceFeatures.UpdateAsync(feature);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Service feature updated: Id={FeatureId}", request.Id);

                    return Result<ServiceFeatureDto>.Success(_mapper.Map<ServiceFeatureDto>(feature));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating service feature: {FeatureId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ServiceFeatureDto>.Failure($"Failed to update feature: {ex.Message}");
                }
            }
        }
    }
}