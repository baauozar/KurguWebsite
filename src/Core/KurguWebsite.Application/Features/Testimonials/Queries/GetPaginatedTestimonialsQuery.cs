using AutoMapper;
using AutoMapper.QueryableExtensions;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using MediatR;

namespace KurguWebsite.Application.Features.Testimonials.Queries
{
    public class GetPaginatedTestimonialsQuery : IRequest<Result<PaginatedList<TestimonialDto>>>
    {
        public QueryParameters Params { get; set; }
    }

    public class GetPaginatedTestimonialsQueryHandler : IRequestHandler<GetPaginatedTestimonialsQuery, Result<PaginatedList<TestimonialDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPaginatedTestimonialsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<TestimonialDto>>> Handle(GetPaginatedTestimonialsQuery request, CancellationToken cancellationToken)
        {
            var testimonialsQuery = (await _unitOfWork.Testimonials.GetAsync(
                predicate: null,
                orderBy: q => q.OrderBy(t => t.DisplayOrder),
                includeString: null,
                disableTracking: true))
                .AsQueryable();

            var paginatedList = await PaginatedList<TestimonialDto>.CreateAsync(
                testimonialsQuery.ProjectTo<TestimonialDto>(_mapper.ConfigurationProvider),
                request.Params.PageNumber,
                request.Params.PageSize);

            return Result<PaginatedList<TestimonialDto>>.Success(paginatedList);
        }
    }
}