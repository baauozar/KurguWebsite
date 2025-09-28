using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.Interfaces.Repositories; // IPageUniquenessChecker
using KurguWebsite.Domain.Events;
using MediatR;
using System;

namespace KurguWebsite.Application.Features.Pages.Commands
{
    public class UpdatePageContentCommand : UpdatePageDto, IRequest<Result<PageDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePageContentCommandHandler : IRequestHandler<UpdatePageContentCommand, Result<PageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ISeoService _seo;
        private readonly IPageUniquenessChecker _unique;

        public UpdatePageContentCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ISeoService seo,
            IPageUniquenessChecker unique)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _seo = seo;
            _unique = unique;
        }

        public async Task<Result<PageDto>> Handle(UpdatePageContentCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.Pages.GetByIdAsync(request.Id);
            if (entity is null)
                return Result<PageDto>.Failure("Page not found.");

            // If title changed, regenerate a UNIQUE slug
            var titleChanged = !string.Equals(entity.Title, request.Title, StringComparison.Ordinal);
            if (titleChanged)
            {
                var baseSlug = _seo.SanitizeSlug(_seo.GenerateSlug(request.Title));
                if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "page";

                var candidate = baseSlug;
                var i = 2;
                while (await _unique.PageSlugExistsAsync(candidate, request.Id, cancellationToken))
                    candidate = $"{baseSlug}-{i++}";

                entity.UpdateSlug(candidate);
            }

            // Update other fields
            entity.Update(request.Title, request.PageType??entity.PageType, request.Content);
            entity.UpdateHeroSection(
                request.HeroTitle, request.HeroSubtitle, request.HeroDescription,
                request.HeroBackgroundImage, request.HeroButtonText, request.HeroButtonUrl);
            entity.UpdateSeo(request.MetaTitle, request.MetaDescription, request.MetaKeywords);

            await _unitOfWork.Pages.UpdateAsync(entity);
            await _unitOfWork.CommitAsync(); // use CommitAsync(cancellationToken) if your UoW supports it

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Page, CacheKeys.HomePage), cancellationToken);

            return Result<PageDto>.Success(_mapper.Map<PageDto>(entity));
        }
    }
}
