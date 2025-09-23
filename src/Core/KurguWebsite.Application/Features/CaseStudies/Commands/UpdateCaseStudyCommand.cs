using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class UpdateCaseStudyCommand : UpdateCaseStudyDto, IRequest<Result<CaseStudyDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateCaseStudyCommandHandler : IRequestHandler<UpdateCaseStudyCommand, Result<CaseStudyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdateCaseStudyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<CaseStudyDto>> Handle(UpdateCaseStudyCommand request, CancellationToken cancellationToken)
        {
            var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(request.Id);
            if (caseStudy == null) return Result<CaseStudyDto>.Failure("Case Study not found.");

            caseStudy.Update(request.Title, request.ClientName, request.Description, request.Challenge,request.Solution,request.Result);
            caseStudy.SetModifiedBy(_currentUserService.UserId ?? "System");

            await _unitOfWork.CaseStudies.UpdateAsync(caseStudy);
            await _unitOfWork.CommitAsync();

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.CaseStudies, CacheKeys.FeaturedCaseStudies), cancellationToken);

            return Result<CaseStudyDto>.Success(_mapper.Map<CaseStudyDto>(caseStudy));
        }
    }
}