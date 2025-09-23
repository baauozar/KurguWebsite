using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    public class CreateCaseStudyCommand : CreateCaseStudyDto, IRequest<Result<CaseStudyDto>> { }

    public class CreateCaseStudyCommandHandler : IRequestHandler<CreateCaseStudyCommand, Result<CaseStudyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public CreateCaseStudyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<CaseStudyDto>> Handle(CreateCaseStudyCommand request, CancellationToken cancellationToken)
        {
            var caseStudy = CaseStudy.Create(request.Title, request.ClientName, request.Description,request.ImagePath,request.CompletedDate);
            caseStudy.SetCreatedBy(_currentUserService.UserId ?? "System");

            await _unitOfWork.CaseStudies.AddAsync(caseStudy);
            await _unitOfWork.CommitAsync();

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.CaseStudies, CacheKeys.FeaturedCaseStudies), cancellationToken);

            return Result<CaseStudyDto>.Success(_mapper.Map<CaseStudyDto>(caseStudy));
        }
    }
}