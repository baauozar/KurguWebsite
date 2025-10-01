using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class GetCaseStudyBySlugQuery : IRequest<Result<CaseStudyDto>>
    {
        public string Slug { get; set; }=string.Empty;
    }

    public class GetCaseStudyBySlugQueryHandler : IRequestHandler<GetCaseStudyBySlugQuery, Result<CaseStudyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCaseStudyBySlugQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<CaseStudyDto>> Handle(GetCaseStudyBySlugQuery request, CancellationToken cancellationToken)
        {
            var caseStudy = await _unitOfWork.CaseStudies.GetBySlugAsync(request.Slug);
            if (caseStudy == null) return Result<CaseStudyDto>.Failure("Case Study not found.");

            return Result<CaseStudyDto>.Success(_mapper.Map<CaseStudyDto>(caseStudy));
        }
    }
}