// src/Core/KurguWebsite.Application/Features/AuditLogs/Queries/GetPaginatedAuditLogsQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using MediatR;

namespace KurguWebsite.Application.Features.AuditLogs.Queries
{
    public class GetPaginatedAuditLogsQuery : IRequest<Result<PaginatedList<AuditLogDto>>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }

    public class GetPaginatedAuditLogsQueryHandler
        : IRequestHandler<GetPaginatedAuditLogsQuery, Result<PaginatedList<AuditLogDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetPaginatedAuditLogsQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<AuditLogDto>>> Handle(
            GetPaginatedAuditLogsQuery request,
            CancellationToken ct)
        {
            // Use the repository method that returns PaginatedList<AuditLogDto>
            var paginatedList = await _uow.AuditLogs.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                _mapper.ConfigurationProvider,
                ct);

            return Result<PaginatedList<AuditLogDto>>.Success(paginatedList);
        }
    }
}