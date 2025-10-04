// src/Core/KurguWebsite.Application/Features/AuditLogs/Queries/GetAuditLogsByEntityQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using KurguWebsite.Domain.Specifications;
using MediatR;
using System.Linq; // optional if already present
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KurguWebsite.Application.Features.AuditLogs.Queries
{
    public class GetAuditLogsByEntityQuery : IRequest<Result<List<AuditLogDto>>>
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
    }

    public class GetAuditLogsByEntityQueryHandler
        : IRequestHandler<GetAuditLogsByEntityQuery, Result<List<AuditLogDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAuditLogsByEntityQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public Task<Result<List<AuditLogDto>>> Handle(
            GetAuditLogsByEntityQuery request,
            CancellationToken cancellationToken)
        {
            var spec = new AuditLogsByEntitySpecification(
                request.EntityType,
                request.EntityId);

            // Use synchronous materialization so core layer does not depend on EF Core async extensions
            var logs = _uow.AuditLogs.Entities
                .Where(spec.Criteria)
                .ToList();

            var dtos = _mapper.Map<List<AuditLogDto>>(logs);

            return Task.FromResult(Result<List<AuditLogDto>>.Success(dtos));
        }
    }
}