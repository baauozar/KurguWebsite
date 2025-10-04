// src/Core/KurguWebsite.Application/Features/ProcessSteps/Queries/SearchProcessStepsQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.ProcessSteps.Queries
{
    public class SearchProcessStepsQuery : IRequest<Result<List<ProcessStepDto>>>
    {
        public string? SearchTerm { get; set; }
    }

    public class SearchProcessStepsQueryHandler
        : IRequestHandler<SearchProcessStepsQuery, Result<List<ProcessStepDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SearchProcessStepsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ProcessStepDto>>> Handle(
            SearchProcessStepsQuery request,
            CancellationToken ct)
        {
            var spec = new ProcessStepSearchSpecification(request.SearchTerm);
            var steps = await _unitOfWork.ProcessSteps.ListAsync(spec, ct);
            var mappedSteps = _mapper.Map<List<ProcessStepDto>>(steps);

            return Result<List<ProcessStepDto>>.Success(mappedSteps);
        }
    }
}