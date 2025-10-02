// src/Core/KurguWebsite.Application/Features/CaseStudies/Queries/GetCaseStudiesByServiceQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Specifications;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.CaseStudies.Queries
{
    public class GetCaseStudiesByServiceQuery : IRequest<Result<List<CaseStudyDto>>>
    {
        public Guid ServiceId { get; set; }
    }

    public class GetCaseStudiesByServiceQueryHandler
        : IRequestHandler<GetCaseStudiesByServiceQuery, Result<List<CaseStudyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCaseStudiesByServiceQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<CaseStudyDto>>> Handle(
            GetCaseStudiesByServiceQuery request,
            CancellationToken cancellationToken)
        {
            // Use specification
            var spec = new CaseStudiesByServiceSpecification(request.ServiceId);
            var caseStudies = await _unitOfWork.CaseStudies.ListAsync(spec, cancellationToken);

            var mappedCaseStudies = _mapper.Map<List<CaseStudyDto>>(caseStudies);

            return Result<List<CaseStudyDto>>.Success(mappedCaseStudies);
        }
    }
}