// src/Core/KurguWebsite.Application/Features/Services/Commands/CreateServiceFeatureCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class CreateServiceFeatureCommand : IRequest<Result<ServiceFeatureDto>>
    {
        public Guid ServiceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
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
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId);
                    if (service == null)
                    {
                        return Result<ServiceFeatureDto>.Failure(
                            "Service not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    var entity = ServiceFeature.Create(
                        request.ServiceId,
                        request.Title,
                        request.Description,
                        request.IconClass,
                        request.DisplayOrder
                    );

                    entity.CreatedBy = _currentUserService.UserId ?? "System";
                    entity.CreatedDate = DateTime.UtcNow;

                    await _unitOfWork.ServiceFeatures.AddAsync(entity);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Service feature created: Id={FeatureId}, ServiceId={ServiceId}",
                        entity.Id, request.ServiceId);

                    var dto = _mapper.Map<ServiceFeatureDto>(entity);
                    return Result<ServiceFeatureDto>.Success(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating service feature");
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ServiceFeatureDto>.Failure($"Failed to create feature: {ex.Message}");
                }
            }
        }
    }
}