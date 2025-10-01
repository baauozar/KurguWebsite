using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class UpdateServiceCommand : UpdateServiceDto, IRequest<Result<ServiceDto>>
    {
       new public Guid Id { get; set; }
    }

    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ISeoService _seo;
        private readonly IServiceUniquenessChecker _unique;

        public UpdateServiceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator, ISeoService seo, IServiceUniquenessChecker unique)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _seo = seo;
            _unique = unique;
        }

        public async Task<Result<ServiceDto>> Handle(UpdateServiceCommand req, CancellationToken ct)
        {
            // NOTE: use _unitOfWork (not _uow)
            var entity = await _unitOfWork.Services.GetByIdAsync(req.Id);
            if (entity is null) return Result<ServiceDto>.Failure("Service not found.");

            var titleChanged = !string.Equals(entity.Title, req.Title, StringComparison.Ordinal);
            if (titleChanged)
            {
                if (await _unique.TitleExistsAsync(req.Title, req.Id, ct))
                    return Result<ServiceDto>.Failure("A service with this title already exists.");

                // Build unique slug from new title
                var baseSlug = _seo.SanitizeSlug(_seo.GenerateSlug(req.Title));
                if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "service";

                var candidate = baseSlug;
                var i = 2;
                while (await _unique.SlugExistsAsync(candidate, req.Id, ct))
                    candidate = $"{baseSlug}-{i++}";

                entity.UpdateSlug(candidate); // domain method you added
            }

            entity.Update(
                title: req.Title,
                description: req.Description,
                shortDescription: req.ShortDescription,
                fullDescription: req.FullDescription,
                iconPath: req.IconPath,
                category: req.Category
            );

            await _unitOfWork.Services.UpdateAsync(entity); // match your signatures (no ct)
            await _unitOfWork.CommitAsync();                // match your signatures (no ct)

            await _mediator.Publish(new CacheInvalidationEvent(
                CacheKeys.Services, CacheKeys.FeaturedServices, CacheKeys.HomePage), ct);

            return Result<ServiceDto>.Success(_mapper.Map<ServiceDto>(entity));
        }
    }
}