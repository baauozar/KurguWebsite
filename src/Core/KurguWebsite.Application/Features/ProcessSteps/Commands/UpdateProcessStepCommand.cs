using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.ProcessSteps.Commands
{
    public class UpdateProcessStepCommand : UpdateProcessStepDto, IRequest<Result<ProcessStepDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateProcessStepCommandHandler : IRequestHandler<UpdateProcessStepCommand, Result<ProcessStepDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdateProcessStepCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<ProcessStepDto>> Handle(UpdateProcessStepCommand request, CancellationToken cancellationToken)
        {
            var processStep = await _unitOfWork.ProcessSteps.GetByIdAsync(request.Id);
            if (processStep == null) return Result<ProcessStepDto>.Failure("Process Step not found.");

            processStep.Update(request.Title, request.Description, request.IconClass);
            processStep.SetModifiedBy(_currentUserService.UserId ?? "System");

            await _unitOfWork.ProcessSteps.UpdateAsync(processStep);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.ProcessSteps, CacheKeys.HomePage, CacheKeys.ServicesPage), cancellationToken);

            return Result<ProcessStepDto>.Success(_mapper.Map<ProcessStepDto>(processStep));
        }
    }
}