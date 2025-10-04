// src/Core/KurguWebsite.Application/Features/AuditLogs/Queries/SearchAuditLogsQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.AuditLogs.Queries
{
    public class SearchAuditLogsQuery : IRequest<Result<PaginatedList<AuditLogDto>>>
    {
        public string? UserId { get; set; }
        public string? EntityType { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
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
            // Spec with sorting + paging
            var spec = new AuditLogSearchSpecification(
                userId: request.UserId,
                entityType: request.EntityType,
                action: request.Action,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                searchTerm: request.SearchTerm,
                sortColumn: request.SortColumn,
                sortOrder: request.SortOrder,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            // Count spec (no paging)
            var countSpec = new AuditLogSearchCountSpecification(
                userId: request.UserId,
                entityType: request.EntityType,
                action: request.Action,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                searchTerm: request.SearchTerm);

            // ✅ Use spec-aware repository methods
            var logs = _uow.AuditLogs.Entities.Where(spec.Criteria).ToList();
            var total = _uow.AuditLogs.Entities.Where(countSpec.Criteria).Count();

            var dtos = _mapper.Map<List<AuditLogDto>>(logs);
            var page = new PaginatedList<AuditLogDto>(dtos, total, request.PageNumber, request.PageSize);

            return Result<PaginatedList<AuditLogDto>>.Success(page);
        }
    }
}
