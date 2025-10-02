using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.Interfaces.Repositories; // ICaseStudyUniquenessChecker
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

        // NEW deps
        private readonly ISeoService _seo;
        private readonly ICaseStudyUniquenessChecker _unique;

        public UpdateCaseStudyCommandHandler(
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

        public async Task<Result<CaseStudyDto>> Handle(UpdateCaseStudyCommand req, CancellationToken ct)
        {
            var entity = await _unitOfWork.CaseStudies.GetByIdAsync(req.Id);
            if (entity is null)
                return Result<CaseStudyDto>.Failure("Case Study not found.");

            var titleChanged = !string.Equals(entity.Title, req.Title, StringComparison.Ordinal);
            if (titleChanged)
            {
                var baseSlug = _seo.SanitizeSlug(_seo.GenerateSlug(req.Title));
                if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "case-study";

                var candidate = baseSlug;
                var i = 2;
                while (await _unique.SlugExistsAsync(candidate, req.Id, ct))
                    candidate = $"{baseSlug}-{i++}";

                entity.UpdateSlug(candidate);
            }

            entity.Update(
                title: req.Title,
                clientName: req.ClientName,
                description: req.Description,
                challenge: req.Challenge,
                solution: req.Solution,
                result: req.Result
            );

            // Track who modified
            entity.LastModifiedBy = _currentUserService.UserId ?? "System";
            entity.LastModifiedDate = DateTime.UtcNow;

            await _unitOfWork.CaseStudies.UpdateAsync(entity);
            await _unitOfWork.CommitAsync();

            await _mediator.Publish(
                new CacheInvalidationEvent(CacheKeys.CaseStudies, CacheKeys.FeaturedCaseStudies),
                ct
            );

            return Result<CaseStudyDto>.Success(_mapper.Map<CaseStudyDto>(entity));
        }
    }
}
