using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using MediatR;

namespace KurguWebsite.Application.Features.ProcessSteps.Queries
{
    public class GetActiveProcessStepsQuery : IRequest<Result<List<ProcessStepDto>>> { }

    public class GetActiveProcessStepsQueryHandler : IRequestHandler<GetActiveProcessStepsQuery, Result<List<ProcessStepDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetActiveProcessStepsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ProcessStepDto>>> Handle(GetActiveProcessStepsQuery request, CancellationToken cancellationToken)
        {
            var steps = await _unitOfWork.ProcessSteps.GetActiveStepsOrderedAsync();
            var mappedSteps = _mapper.Map<List<ProcessStepDto>>(steps);
            return Result<List<ProcessStepDto>>.Success(mappedSteps);
        }
    }
}