// src/Core/KurguWebsite.Application/Features/CaseStudies/Commands/UpdateCaseStudyMetricCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class UpdateCaseStudyMetricCommand : IRequest<Result<CaseStudyMetricDto>>
    {
        public Guid Id { get; set; }
        public UpdateCaseStudyMetricDto UpdateCaseStudyMetricDto { get; set; }
    }

    public class UpdateCaseStudyMetricCommandHandler : IRequestHandler<UpdateCaseStudyMetricCommand, Result<CaseStudyMetricDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCaseStudyMetricCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CaseStudyMetricDto>> Handle(UpdateCaseStudyMetricCommand request, CancellationToken cancellationToken)
        {
            var caseStudyMetric = await _unitOfWork.CaseStudyMetrics.GetByIdAsync(request.Id);

            if (caseStudyMetric == null)
            {
                return Result<CaseStudyMetricDto>.Failure("Case Study Metric not found.");
            }

            caseStudyMetric.Update(
                request.UpdateCaseStudyMetricDto.MetricName,
                request.UpdateCaseStudyMetricDto.MetricValue,
                request.UpdateCaseStudyMetricDto.Icon);

            // Track who modified
            caseStudyMetric.LastModifiedBy = _currentUserService.UserId ?? "System";
            caseStudyMetric.LastModifiedDate = DateTime.UtcNow;

            await _unitOfWork.CaseStudyMetrics.UpdateAsync(caseStudyMetric);
            await _unitOfWork.CommitAsync();

            return Result<CaseStudyMetricDto>.Success(_mapper.Map<CaseStudyMetricDto>(caseStudyMetric));
        }
    }
}