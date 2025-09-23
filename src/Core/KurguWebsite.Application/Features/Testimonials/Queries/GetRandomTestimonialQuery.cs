using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using MediatR;

namespace KurguWebsite.Application.Features.Testimonials.Queries
{
    public class GetRandomTestimonialQuery : IRequest<Result<TestimonialDto>> { }

    public class GetRandomTestimonialQueryHandler : IRequestHandler<GetRandomTestimonialQuery, Result<TestimonialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetRandomTestimonialQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<TestimonialDto>> Handle(GetRandomTestimonialQuery request, CancellationToken cancellationToken)
        {
            var testimonial = await _unitOfWork.Testimonials.GetRandomTestimonialAsync();
            if (testimonial == null) return Result<TestimonialDto>.Failure("No testimonials available.");

            return Result<TestimonialDto>.Success(_mapper.Map<TestimonialDto>(testimonial));
        }
    }
}