// src/Core/KurguWebsite.Application/Features/ProcessSteps/Queries/GetProcessStepsByOrderQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.ProcessSteps.Queries
{
    public class GetProcessStepsByOrderQuery : IRequest<Result<List<ProcessStepDto>>> { }

    public class GetProcessStepsByOrderQueryHandler
        : IRequestHandler<GetProcessStepsByOrderQuery, Result<List<ProcessStepDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetProcessStepsByOrderQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ProcessStepDto>>> Handle(
            GetProcessStepsByOrderQuery request,
            CancellationToken ct)
        {
            var spec = new ProcessStepsByOrderSpecification();
            var steps = await _unitOfWork.ProcessSteps.ListAsync(spec, ct);
            var mappedSteps = _mapper.Map<List<ProcessStepDto>>(steps);

            return Result<List<ProcessStepDto>>.Success(mappedSteps);
        }
    }
}