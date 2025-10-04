// src/Core/KurguWebsite.Application/Features/CaseStudies/Queries/GetAllCaseStudiesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class GetAllCaseStudiesQuery : IRequest<Result<List<CaseStudyDto>>> { }

    public class GetAllCaseStudiesQueryHandler
        : IRequestHandler<GetAllCaseStudiesQuery, Result<List<CaseStudyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllCaseStudiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<CaseStudyDto>>> Handle(
            GetAllCaseStudiesQuery request,
            CancellationToken ct)
        {
            var caseStudies = await _unitOfWork.CaseStudies.GetActiveCaseStudiesAsync();
            var mappedCaseStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies);

            return Result<List<CaseStudyDto>>.Success(mappedCaseStudies);
        }
    }
}