using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.ProcessSteps.Commands
{
    public class CreateProcessStepCommand : CreateProcessStepDto, IRequest<Result<ProcessStepDto>> { }

    public class CreateProcessStepCommandHandler : IRequestHandler<CreateProcessStepCommand, Result<ProcessStepDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public CreateProcessStepCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<ProcessStepDto>> Handle(CreateProcessStepCommand request, CancellationToken cancellationToken)
        {
            var processStep = ProcessStep.Create(request.StepNumber, request.Title, request.Description);
            processStep.SetCreatedBy(_currentUserService.UserId ?? "System");

            await _unitOfWork.ProcessSteps.AddAsync(processStep);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.ProcessSteps, CacheKeys.HomePage, CacheKeys.ServicesPage), cancellationToken);

            return Result<ProcessStepDto>.Success(_mapper.Map<ProcessStepDto>(processStep));
        }
    }
}