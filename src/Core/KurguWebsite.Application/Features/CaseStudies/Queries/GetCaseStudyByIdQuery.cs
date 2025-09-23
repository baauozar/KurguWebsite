using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class GetCaseStudyByIdQuery : IRequest<Result<CaseStudyDto>>
    {
        public Guid Id { get; set; }
    }
    public class GetCaseStudyByIdQueryHandler : IRequestHandler<GetCaseStudyByIdQuery, Result<CaseStudyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetCaseStudyByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<CaseStudyDto>> Handle(GetCaseStudyByIdQuery request, CancellationToken cancellationToken)
        {
            var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(request.Id);
            if (caseStudy == null) return Result<CaseStudyDto>.Failure("Case Study not found.");
            return Result<CaseStudyDto>.Success(_mapper.Map<CaseStudyDto>(caseStudy));
        }
    }
}