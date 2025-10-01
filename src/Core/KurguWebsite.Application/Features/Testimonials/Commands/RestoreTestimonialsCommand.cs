using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.Testimonials.Commands
{
    public class RestoreTestimonialsCommand : IRequest<Result<TestimonialDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestoreTestimonialsCommandHandler
        : IRequestHandler<RestoreTestimonialsCommand, Result<TestimonialDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public RestoreTestimonialsCommandHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator)
        {
            _uow = uow; _mapper = mapper; _mediator = mediator;
        }

        public async Task<Result<TestimonialDto>> Handle(RestoreTestimonialsCommand request, CancellationToken ct)
        {
            // Load INCLUDING soft-deleted row
            var entity = await _uow.Services.GetByIdIncludingDeletedAsync(request.Id);
            if (entity is null) return Result<TestimonialDto>.Failure("Process not found.");

            if (entity.IsDeleted)
            {
                // Soft restore
                await _uow.Services.RestoreAsync(entity);
                await _uow.CommitAsync(ct);

                // Invalidate caches as you do elsewhere
                await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Services, CacheKeys.FeaturedServices), ct);
            }

            return Result<TestimonialDto>.Success(_mapper.Map<TestimonialDto>(entity));
        }
    }
}