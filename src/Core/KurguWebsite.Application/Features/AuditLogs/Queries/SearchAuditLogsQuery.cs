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
            var spec = new AuditLogSearchSpecification(
                userId: request.UserId,
                entityType: request.EntityType,
                action: request.Action,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            var logs = _uow.AuditLogs.Entities.Where(spec.Criteria).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();

            var countSpec = new AuditLogSearchSpecification(
                userId: request.UserId,
                entityType: request.EntityType,
                action: request.Action,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                pageNumber: 1,
                pageSize: int.MaxValue);
            var totalCount = _uow.AuditLogs.Entities
             .Where(countSpec.Criteria)
             .Count();
            var mappedLogs = _mapper.Map<List<AuditLogDto>>(logs);

            var paginatedList = new PaginatedList<AuditLogDto>(
                mappedLogs,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedList<AuditLogDto>>.Success(paginatedList);
        }
    }
}