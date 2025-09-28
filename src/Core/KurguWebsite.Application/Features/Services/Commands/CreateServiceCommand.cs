using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Interfaces.Repositories; // IServiceUniquenessChecker
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class CreateServiceCommand : CreateServiceDto, IRequest<Result<ServiceDto>> { }

    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        // NEW deps for slug/uniqueness
        private readonly ISeoService _seo;
        private readonly IServiceUniquenessChecker _unique;

        public CreateServiceCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ISeoService seo,
            IServiceUniquenessChecker unique)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _seo = seo;
            _unique = unique;
        }

        public async Task<Result<ServiceDto>> Handle(CreateServiceCommand req, CancellationToken ct)
        {
            // Title idempotency
            if (await _unique.TitleExistsAsync(req.Title, null, ct))
                return Result<ServiceDto>.Failure("A service with this title already exists.");

            // Create aggregate (Create sets a base slug; we'll overwrite with a unique one)
            var entity = Service.Create(
                title: req.Title,
                description: req.Description,
                shortDescription: req.ShortDescription,
                iconPath: req.IconPath,
                category: req.Category
            );

            // Unique slug
            var baseSlug = _seo.SanitizeSlug(_seo.GenerateSlug(req.Title));
            if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "service";

            var candidate = baseSlug;
            var i = 2;
            while (await _unique.SlugExistsAsync(candidate, null, ct))
                candidate = $"{baseSlug}-{i++}";

            entity.UpdateSlug(candidate); // domain method (ensure it exists as discussed)

            // Optional features
            if (req.Features is { Count: > 0 })
            {
                foreach (var f in req.Features)
                {
                    // overload in Service: AddFeature(string title, string desc, string? iconClass = null, int displayOrder = 0)
                    // replace this inside the foreach:
                    entity.AddFeature(f.Title, f.Description, f.IconClass);

                }
            }

            await _unitOfWork.Services.AddAsync(entity);         // matches your 1-arg signature
            await _unitOfWork.CommitAsync(ct);                   // your CommitAsync(cancellationToken)

            await _mediator.Publish(new CacheInvalidationEvent(
                CacheKeys.Services, CacheKeys.FeaturedServices, CacheKeys.HomePage), ct);

            return Result<ServiceDto>.Success(_mapper.Map<ServiceDto>(entity));
        }
    }
}
