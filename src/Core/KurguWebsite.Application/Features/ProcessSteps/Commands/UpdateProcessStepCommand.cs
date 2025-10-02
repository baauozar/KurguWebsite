// src/Core/KurguWebsite.Application/Features/ProcessSteps/Commands/UpdateProcessStepCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.ProcessSteps.Commands
{
    public class UpdateProcessStepCommand : UpdateProcessStepDto,
        IRequest<Result<ProcessStepDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateProcessStepCommandHandler
        : IRequestHandler<UpdateProcessStepCommand, Result<ProcessStepDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateProcessStepCommandHandler> _logger;

        public UpdateProcessStepCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<UpdateProcessStepCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<ProcessStepDto>> Handle(
            UpdateProcessStepCommand req,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var processStep = await _unitOfWork.ProcessSteps.GetByIdAsync(req.Id);
                    if (processStep is null)
                    {
                        return Result<ProcessStepDto>.Failure(
                            "Process step not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    processStep.Update(
                       
                        req.Title ?? processStep.Title,
                        req.Description ?? processStep.Description,
                        req.IconClass ?? processStep.IconClass

                    );

                    processStep.LastModifiedBy = _currentUserService.UserId ?? "System";
                    processStep.LastModifiedDate = DateTime.UtcNow;

                    await _unitOfWork.ProcessSteps.UpdateAsync(processStep);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Process step updated: Id={ProcessStepId}", req.Id);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.ProcessSteps,
                            CacheKeys.HomePage),
                        ct);

                    return Result<ProcessStepDto>.Success(_mapper.Map<ProcessStepDto>(processStep));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating process step: {ProcessStepId}", req.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ProcessStepDto>.Failure($"Failed to update process step: {ex.Message}");
                }
            }
        }
    }
}