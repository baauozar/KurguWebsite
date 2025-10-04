// src/Core/KurguWebsite.Application/Features/CaseStudies/Queries/GetAllActiveMetricsQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class GetAllActiveMetricsQuery : IRequest<Result<List<CaseStudyMetricDto>>> { }

    public class GetAllActiveMetricsQueryHandler
        : IRequestHandler<GetAllActiveMetricsQuery, Result<List<CaseStudyMetricDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllActiveMetricsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<CaseStudyMetricDto>>> Handle(
            GetAllActiveMetricsQuery request,
            CancellationToken ct)
        {
            var spec = new ActiveMetricsSpecification();
            var metrics = await _unitOfWork.CaseStudyMetrics.ListAsync(spec, ct);
            var dtos = _mapper.Map<List<CaseStudyMetricDto>>(metrics);

            return Result<List<CaseStudyMetricDto>>.Success(dtos);
        }
    }
}