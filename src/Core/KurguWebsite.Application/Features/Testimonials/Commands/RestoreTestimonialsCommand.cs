using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.Testimonials.Commands
{
    public class RestoreTestimonialCommand : IRequest<Result<TestimonialDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreTestimonialCommandHandler
        : IRequestHandler<RestoreTestimonialCommand, Result<TestimonialDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public RestoreTestimonialCommandHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator, ICurrentUserService currentUserService)
        {
            _uow = uow;
            _mapper = mapper;
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public async Task<Result<TestimonialDto>> Handle(RestoreTestimonialCommand request, CancellationToken ct)
        {
            // FIX: Use correct repository
            var entity = await _uow.Testimonials.GetByIdIncludingDeletedAsync(request.Id);
            if (entity is null) return Result<TestimonialDto>.Failure("Testimonial not found.");

            if (entity.IsDeleted)
            {
                await _uow.Testimonials.RestoreAsync(entity);

                // Track who restored
                entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                entity.LastModifiedDate = DateTime.UtcNow;

                await _uow.CommitAsync(ct);

                await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Testimonials, CacheKeys.ActiveTestimonials, CacheKeys.HomePage), ct);
            }

            return Result<TestimonialDto>.Success(_mapper.Map<TestimonialDto>(entity));
        }
    }
}