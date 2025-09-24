using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.Testimonials.Commands
{
    public class CreateTestimonialCommand : CreateTestimonialDto, IRequest<Result<TestimonialDto>> { }

    public class CreateTestimonialCommandHandler : IRequestHandler<CreateTestimonialCommand, Result<TestimonialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public CreateTestimonialCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<TestimonialDto>> Handle(CreateTestimonialCommand request, CancellationToken cancellationToken)
        {
            var testimonial = Testimonial.Create(request.ClientName, request.ClientTitle, request.CompanyName, request.Content, request.Rating);
   

            await _unitOfWork.Testimonials.AddAsync(testimonial);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Testimonials, CacheKeys.ActiveTestimonials, CacheKeys.HomePage), cancellationToken);

            return Result<TestimonialDto>.Success(_mapper.Map<TestimonialDto>(testimonial));
        }
    }
}