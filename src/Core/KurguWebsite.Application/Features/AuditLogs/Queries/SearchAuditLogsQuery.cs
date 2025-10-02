// src/Core/KurguWebsite.Application/Features/AuditLogs/Queries/SearchAuditLogsQuery.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using MediatR;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace KurguWebsite.Application.Features.AuditLogs.Queries
{
    public class SearchAuditLogsQuery : IRequest<Result<PaginatedList<AuditLogDto>>>
    {
        public QueryParameters Params { get; set; } = new();
        public string? UserId { get; set; }
        public string? EntityType { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class SearchAuditLogsQueryHandler
        : IRequestHandler<SearchAuditLogsQuery, Result<PaginatedList<AuditLogDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SearchAuditLogsQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<AuditLogDto>>> Handle(
            SearchAuditLogsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _uow.AuditLogs.Entities.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                query = query.Where(a => a.UserId == request.UserId);
            }

            if (!string.IsNullOrWhiteSpace(request.EntityType))
            {
                query = query.Where(a => a.EntityType == request.EntityType);
            }

            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                query = query.Where(a => a.Action.Contains(request.Action));
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= request.ToDate.Value);
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(request.Params.SearchTerm))
            {
                var term = request.Params.SearchTerm.Trim().ToLower();
                query = query.Where(a =>
                    a.UserName.ToLower().Contains(term) ||
                    a.Action.ToLower().Contains(term) ||
                    a.EntityType.ToLower().Contains(term) ||
                    a.EntityId.Contains(term)
                );
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(request.Params.SortColumn))
            {
                var sortOrder = request.Params.SortOrder?.ToLower() == "desc"
                    ? "descending"
                    : "ascending";
                query = query.OrderBy($"{request.Params.SortColumn} {sortOrder}");
            }
            else
            {
                query = query.OrderByDescending(a => a.Timestamp);
            }

            var paginatedList = await PaginatedList<AuditLogDto>.CreateAsync(
                query.ProjectTo<AuditLogDto>(_mapper.ConfigurationProvider),
                request.Params.PageNumber,
                request.Params.PageSize
            );

            return Result<PaginatedList<AuditLogDto>>.Success(paginatedList);
        }
    }
}