// src/Core/KurguWebsite.Application/Features/ProcessSteps/Commands/CreateProcessStepCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.ProcessSteps.Commands
{
    public class CreateProcessStepCommand : CreateProcessStepDto,
        IRequest<Result<ProcessStepDto>>
    { }

    public class CreateProcessStepCommandHandler
        : IRequestHandler<CreateProcessStepCommand, Result<ProcessStepDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateProcessStepCommandHandler> _logger;

        public CreateProcessStepCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<CreateProcessStepCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<ProcessStepDto>> Handle(
            CreateProcessStepCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var maxOrder = await _unitOfWork.Services.GetAllAsync(ct)
             .ContinueWith(t => t.Result.Any() ? t.Result.Max(s => s.DisplayOrder) : 0);
                    var nextDisplayOrder = maxOrder + 1; var processStep = ProcessStep.Create(
                 
                        title: request.Title,
                        description: request.Description,
                        iconClass: request.IconClass??string.Empty

                    );
                    processStep.SetDisplayOrder(nextDisplayOrder);
                    processStep.CreatedBy = _currentUserService.UserId ?? "System";
                    processStep.CreatedDate = DateTime.UtcNow;

                    await _unitOfWork.ProcessSteps.AddAsync(processStep);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Process step created: Id={ProcessStepId}, Title={Title}",
                        processStep.Id, processStep.Title);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.ProcessSteps,
                            CacheKeys.HomePage),
                        ct);

                    return Result<ProcessStepDto>.Success(_mapper.Map<ProcessStepDto>(processStep));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating process step: {Title}", request.Title);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<ProcessStepDto>.Failure($"Failed to create process step: {ex.Message}");
                }
            }
        }
    }
}