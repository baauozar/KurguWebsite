// src/Core/KurguWebsite.Application/Features/ProcessSteps/Queries/GetAllProcessStepsQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.ProcessSteps.Queries
{
    public class GetAllProcessStepsQuery : IRequest<Result<List<ProcessStepDto>>> { }

    public class GetAllProcessStepsQueryHandler
        : IRequestHandler<GetAllProcessStepsQuery, Result<List<ProcessStepDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetAllProcessStepsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<List<ProcessStepDto>>> Handle(
            GetAllProcessStepsQuery request,
            CancellationToken ct)
        {
            var cachedSteps = await _cacheService.GetAsync<List<ProcessStepDto>>(CacheKeys.ProcessSteps);
            if (cachedSteps != null)
            {
                return Result<List<ProcessStepDto>>.Success(cachedSteps);
            }

            var spec = new ActiveProcessStepsSpecification();
            var processSteps = await _unitOfWork.ProcessSteps.ListAsync(spec, ct);
            var mappedSteps = _mapper.Map<List<ProcessStepDto>>(processSteps);

            await _cacheService.SetAsync(
                CacheKeys.ProcessSteps,
                mappedSteps,
                TimeSpan.FromMinutes(30));

            return Result<List<ProcessStepDto>>.Success(mappedSteps);
        }
    }
}