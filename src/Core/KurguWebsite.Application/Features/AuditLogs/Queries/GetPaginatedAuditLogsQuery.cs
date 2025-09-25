using AutoMapper;
using KurguWebsite.Application.Common.Extensions;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using KurguWebsite.Application.Mappings;
using KurguWebsite.Domain.Entities;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.AuditLogs.Queries
{
    public class GetPaginatedAuditLogsQuery : IRequest<PaginatedList<AuditLogDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }

    public class GetPaginatedAuditLogsQueryHandler : IRequestHandler<GetPaginatedAuditLogsQuery, PaginatedList<AuditLogDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPaginatedAuditLogsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedList<AuditLogDto>> Handle(GetPaginatedAuditLogsQuery request, CancellationToken cancellationToken)
        {
            // CORRECTED: Use the 'Entities' property to build the query efficiently
            return await _unitOfWork.AuditLogs.Entities
       .OrderByDescending(x => x.Timestamp)
       .ToPaginatedListAsync<AuditLog, AuditLogDto>(
           _mapper.ConfigurationProvider,
           request.PageNumber,
           request.PageSize);
        }
    }
}