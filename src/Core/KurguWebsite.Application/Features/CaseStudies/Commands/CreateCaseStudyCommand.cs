using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.Interfaces.Repositories; // ICaseStudyUniquenessChecker
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
        private readonly ISeoService _seo;
        private readonly ICaseStudyUniquenessChecker _unique;

        public CreateCaseStudyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ISeoService seo,
            ICaseStudyUniquenessChecker unique)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _seo = seo;
            _unique = unique;
        }

        public async Task<Result<CaseStudyDto>> Handle(CreateCaseStudyCommand request, CancellationToken cancellationToken)
        {
            var entity = CaseStudy.Create(
                title: request.Title,
                clientName: request.ClientName,
                description: request.Description,
                imagePath: request.ImagePath,
                completedDate: request.CompletedDate
            );

            // Track who created
            entity.CreatedBy = _currentUserService.UserId ?? "System";
            entity.CreatedDate = DateTime.UtcNow;

            var canon = _seo.SanitizeSlug(_seo.GenerateSlug(request.Title));
            if (string.IsNullOrWhiteSpace(canon)) canon = "case-study";

            var candidate = canon;
            var i = 2;
            while (await _unique.SlugExistsAsync(candidate, null, cancellationToken))
                candidate = $"{canon}-{i++}";

            entity.UpdateSlug(candidate);

            await _unitOfWork.CaseStudies.AddAsync(entity);
            await _unitOfWork.CommitAsync();

            await _mediator.Publish(
                new CacheInvalidationEvent(CacheKeys.CaseStudies, CacheKeys.FeaturedCaseStudies),
                cancellationToken
            );

            return Result<CaseStudyDto>.Success(_mapper.Map<CaseStudyDto>(entity));
        }
    }
}
