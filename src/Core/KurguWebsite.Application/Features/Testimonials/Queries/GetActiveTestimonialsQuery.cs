// src/Core/KurguWebsite.Application/Features/Testimonials/Queries/GetActiveTestimonialsQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Testimonials.Queries
{
    public class GetActiveTestimonialsQuery : IRequest<Result<List<TestimonialDto>>> { }

    public class GetActiveTestimonialsQueryHandler
        : IRequestHandler<GetActiveTestimonialsQuery, Result<List<TestimonialDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetActiveTestimonialsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<List<TestimonialDto>>> Handle(
            GetActiveTestimonialsQuery request,
            CancellationToken ct)
        {
            var cachedTestimonials = await _cacheService.GetAsync<List<TestimonialDto>>(
                CacheKeys.ActiveTestimonials);

            if (cachedTestimonials != null)
            {
                return Result<List<TestimonialDto>>.Success(cachedTestimonials);
            }

            var spec = new ActiveTestimonialsSpecification();
            var testimonials = await _unitOfWork.Testimonials.ListAsync(spec, ct);
            var mappedTestimonials = _mapper.Map<List<TestimonialDto>>(testimonials);

            await _cacheService.SetAsync(
                CacheKeys.ActiveTestimonials,
                mappedTestimonials,
                TimeSpan.FromMinutes(30));

            return Result<List<TestimonialDto>>.Success(mappedTestimonials);
        }
    }
}