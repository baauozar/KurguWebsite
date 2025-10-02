// src/Core/KurguWebsite.Application/Features/CaseStudies/Queries/GetFeaturedCaseStudiesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class GetFeaturedCaseStudiesQuery : IRequest<Result<List<CaseStudyDto>>> { }

    public class GetFeaturedCaseStudiesQueryHandler
        : IRequestHandler<GetFeaturedCaseStudiesQuery, Result<List<CaseStudyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetFeaturedCaseStudiesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<List<CaseStudyDto>>> Handle(
            GetFeaturedCaseStudiesQuery request,
            CancellationToken ct)
        {
            var cachedStudies = await _cacheService.GetAsync<List<CaseStudyDto>>(
                CacheKeys.FeaturedCaseStudies);

            if (cachedStudies != null)
            {
                return Result<List<CaseStudyDto>>.Success(cachedStudies);
            }

            var spec = new FeaturedCaseStudiesSpecification();
            var caseStudies = await _unitOfWork.CaseStudies.ListAsync(spec, ct);
            var mappedStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies);

            await _cacheService.SetAsync(
                CacheKeys.FeaturedCaseStudies,
                mappedStudies,
                TimeSpan.FromMinutes(30));

            return Result<List<CaseStudyDto>>.Success(mappedStudies);
        }
    }
}