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
            var query = _unitOfWork.Testimonials
                .GetAllQueryable()
                .Where(t => t.IsActive); // only active testimonials

            // Optional search term filter
            if (!string.IsNullOrWhiteSpace(request.Params.SearchTerm))
            {
                var term = request.Params.SearchTerm.ToLower();
                query = query.Where(t =>
                    t.ClientName.ToLower().Contains(term) ||
                    t.ClientTitle.ToLower().Contains(term) ||
                    t.CompanyName.ToLower().Contains(term)
                );
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(request.Params.SortColumn))
            {
                if (request.Params.SortColumn.Equals("DisplayOrder", StringComparison.OrdinalIgnoreCase))
                {
                    query = request.Params.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.DisplayOrder)
                        : query.OrderBy(t => t.DisplayOrder);
                }
                else
                {
                    query = query.OrderBy(t => t.CreatedDate); // default
                }
            }
            else
            {
                query = query.OrderBy(t => t.DisplayOrder);
            }

            var paginatedList = await PaginatedList<TestimonialDto>.CreateAsync(
                query.ProjectTo<TestimonialDto>(_mapper.ConfigurationProvider),
                request.Params.PageNumber,
                request.Params.PageSize
            );

            return Result<PaginatedList<TestimonialDto>>.Success(paginatedList);
        }
    }
}