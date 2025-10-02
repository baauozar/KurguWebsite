// src/Core/KurguWebsite.Application/Features/AuditLogs/Queries/GetRecentAuditLogsQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using MediatR;
using System.Linq;

namespace KurguWebsite.Application.Features.AuditLogs.Queries
{
    public class GetRecentAuditLogsQuery : IRequest<Result<List<AuditLogDto>>>
    {
        public int Count { get; set; } = 10;
    }

    public class GetRecentAuditLogsQueryHandler
        : IRequestHandler<GetRecentAuditLogsQuery, Result<List<AuditLogDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetRecentAuditLogsQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<AuditLogDto>>> Handle(
        GetRecentAuditLogsQuery request,
        CancellationToken cancellationToken)
        {
            // Synchronously enumerate the queryable (not ideal for scalability, but doesn't require EF Core in Application)
            var logs = _uow.AuditLogs.Entities
                .OrderByDescending(a => a.Timestamp)
                .Take(request.Count)
                .ToList(); // <- System.Linq.ToList()

            var dtos = _mapper.Map<List<AuditLogDto>>(logs);
            return Result<List<AuditLogDto>>.Success(dtos);
        }
    }
}