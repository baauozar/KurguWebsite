// src/Core/KurguWebsite.Application/Features/Services/Queries/GetServiceFeaturesPagedQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Entities;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries;

public sealed class GetServiceFeaturesPagedQuery : IRequest<Result<PaginatedList<ServiceFeatureDto>>>
{
    public Guid? ServiceId { get; init; }
    public PaginationParams Paging { get; init; } = new();
}

public sealed class GetServiceFeaturesPagedQueryHandler
    : IRequestHandler<GetServiceFeaturesPagedQuery, Result<PaginatedList<ServiceFeatureDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetServiceFeaturesPagedQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<ServiceFeatureDto>>> Handle(
        GetServiceFeaturesPagedQuery request,
        CancellationToken ct)
    {
        // Pull all features via repo (no EF in Application)
        var all = await _uow.ServiceFeatures.GetAllAsync(); // returns IEnumerable<ServiceFeature>

        // to IQueryable for your PaginatedList helper
        var q = all.AsQueryable();

        // Optional filter by ServiceId
        if (request.ServiceId is Guid sid && sid != Guid.Empty)
            q = q.Where(f => f.ServiceId == sid);

        // Optional search on Title/Description
        if (!string.IsNullOrWhiteSpace(request.Paging.SearchTerm))
        {
            var term = request.Paging.SearchTerm.Trim().ToLower();
            q = q.Where(f =>
                (f.Title != null && f.Title.ToLower().Contains(term)) ||
                (f.Description != null && f.Description.ToLower().Contains(term)));
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(request.Paging.SortBy))
        {
            var desc = request.Paging.SortDescending;
            switch (request.Paging.SortBy.Trim().ToLower())
            {
                case "title": q = desc ? q.OrderByDescending(x => x.Title) : q.OrderBy(x => x.Title); break;
                case "displayorder": q = desc ? q.OrderByDescending(x => x.DisplayOrder) : q.OrderBy(x => x.DisplayOrder); break;
                case "createddate": q = desc ? q.OrderByDescending(x => x.CreatedDate) : q.OrderBy(x => x.CreatedDate); break;
                default: q = q.OrderBy(x => x.DisplayOrder); break;
            }
        }
        else
        {
            q = q.OrderBy(x => x.DisplayOrder);
        }

        var page = PaginatedList<ServiceFeature>.Create(
    q,
    request.Paging.PageNumber,
    request.Paging.PageSize
);
        // Map items -> DTOs, keep paging metadata
        var dtoItems = page.Items.Select(_mapper.Map<ServiceFeatureDto>).ToList();
        var dtoPage = new PaginatedList<ServiceFeatureDto>(
            dtoItems, page.TotalCount, page.PageNumber, request.Paging.PageSize);

        return Result<PaginatedList<ServiceFeatureDto>>.Success(dtoPage);
    }
}
