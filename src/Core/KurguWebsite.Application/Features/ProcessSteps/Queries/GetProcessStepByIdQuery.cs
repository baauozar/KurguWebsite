// src/Core/KurguWebsite.Application/Features/ProcessSteps/Queries/GetProcessStepByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using MediatR;

namespace KurguWebsite.Application.Features.ProcessSteps.Queries
{
    public class GetProcessStepByIdQuery : IRequest<Result<ProcessStepDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetProcessStepByIdQueryHandler
        : IRequestHandler<GetProcessStepByIdQuery, Result<ProcessStepDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetProcessStepByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ProcessStepDto>> Handle(
            GetProcessStepByIdQuery request,
            CancellationToken ct)
        {
            var processStep = await _unitOfWork.ProcessSteps.GetByIdAsync(request.Id);
            if (processStep == null)
            {
                return Result<ProcessStepDto>.Failure(
                    "Process step not found.",
                    ErrorCodes.EntityNotFound);
            }

            return Result<ProcessStepDto>.Success(_mapper.Map<ProcessStepDto>(processStep));
        }
    }
}