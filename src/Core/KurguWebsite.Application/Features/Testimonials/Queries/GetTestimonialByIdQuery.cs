// src/Core/KurguWebsite.Application/Features/Testimonials/Queries/GetTestimonialByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using MediatR;

namespace KurguWebsite.Application.Features.Testimonials.Queries
{
    public class GetTestimonialByIdQuery : IRequest<Result<TestimonialDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetTestimonialByIdQueryHandler
        : IRequestHandler<GetTestimonialByIdQuery, Result<TestimonialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTestimonialByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<TestimonialDto>> Handle(
            GetTestimonialByIdQuery request,
            CancellationToken ct)
        {
            var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(request.Id);
            if (testimonial == null)
            {
                return Result<TestimonialDto>.Failure(
                    "Testimonial not found.",
                    ErrorCodes.EntityNotFound);
            }

            return Result<TestimonialDto>.Success(_mapper.Map<TestimonialDto>(testimonial));
        }
    }
}