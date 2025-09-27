using AutoMapper;
using AutoMapper.QueryableExtensions;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetPaginatedServicesQuery : IRequest<Result<PaginatedList<ServiceDto>>>
    {
        public QueryParameters Params { get; set; }
    }

    public class GetPaginatedServicesQueryHandler : IRequestHandler<GetPaginatedServicesQuery, Result<PaginatedList<ServiceDto>>>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public GetPaginatedServicesQueryHandler(IServiceRepository serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ServiceDto>>> Handle(GetPaginatedServicesQuery request, CancellationToken cancellationToken)
        {
            var query = _serviceRepository.GetActiveServicesQueryable();

            // --- Search ---
            if (!string.IsNullOrWhiteSpace(request.Params.SearchTerm))
            {
                var term = request.Params.SearchTerm.ToLower();
                query = query.Where(s => s.Title.ToLower().Contains(term) || s.Description.ToLower().Contains(term));
            }

            // --- Sorting ---
            if (!string.IsNullOrWhiteSpace(request.Params.SortColumn))
            {
                query = request.Params.SortColumn.ToLower() switch
                {
                    "title" => request.Params.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.Title) : query.OrderBy(s => s.Title),
                    "displayorder" => request.Params.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.DisplayOrder) : query.OrderBy(s => s.DisplayOrder),
                    "createddate" => request.Params.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.CreatedDate) : query.OrderBy(s => s.CreatedDate),
                    _ => query.OrderBy(s => s.DisplayOrder)
                };
            }
            else
            {
                query = query.OrderBy(s => s.DisplayOrder);
            }

            // --- Pagination & Projection ---
            var paginatedList = await PaginatedList<ServiceDto>.CreateAsync(
                query.ProjectTo<ServiceDto>(_mapper.ConfigurationProvider),
                request.Params.PageNumber,
                request.Params.PageSize
            );

            return Result<PaginatedList<ServiceDto>>.Success(paginatedList);
        }
    }

}