// src/Core/KurguWebsite.Application/Features/Testimonials/Queries/GetHighRatingTestimonialsQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Testimonials.Queries
{
    public class GetHighRatingTestimonialsQuery : IRequest<Result<List<TestimonialDto>>>
    {
        public int MinRating { get; set; } = 4;
    }

    public class GetHighRatingTestimonialsQueryHandler
        : IRequestHandler<GetHighRatingTestimonialsQuery, Result<List<TestimonialDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetHighRatingTestimonialsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<TestimonialDto>>> Handle(
            GetHighRatingTestimonialsQuery request,
            CancellationToken ct)
        {
            var spec = new HighRatingTestimonialsSpecification(request.MinRating);
            var testimonials = await _unitOfWork.Testimonials.ListAsync(spec, ct);
            var mappedTestimonials = _mapper.Map<List<TestimonialDto>>(testimonials);

            return Result<List<TestimonialDto>>.Success(mappedTestimonials);
        }
    }
}