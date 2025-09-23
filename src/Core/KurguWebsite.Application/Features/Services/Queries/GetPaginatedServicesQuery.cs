using AutoMapper;
using AutoMapper.QueryableExtensions;
using KurguWebsite.Application.Common.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPaginatedServicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ServiceDto>>> Handle(GetPaginatedServicesQuery request, CancellationToken cancellationToken)
        {
            var servicesQuery = (await _unitOfWork.Services.GetAsync(
                     s => s.IsActive,
                     q => q.OrderBy(s => s.DisplayOrder),
                     (string?)null, // Explicitly specify the third parameter as null for the string? overload
                     true))
                     .AsQueryable();

            var paginatedList = await PaginatedList<ServiceDto>.CreateAsync(
                servicesQuery.ProjectTo<ServiceDto>(_mapper.ConfigurationProvider),
                request.Params.PageNumber,
                request.Params.PageSize);

            return Result<PaginatedList<ServiceDto>>.Success(paginatedList);
        }
    }
}