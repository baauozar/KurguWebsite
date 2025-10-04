// src/Core/KurguWebsite.Application/Features/AuditLogs/Queries/GetAuditLogsByDateRangeQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.AuditLogs.Queries
{
    public class GetAuditLogsByDateRangeQuery : IRequest<Result<List<AuditLogDto>>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetAuditLogsByDateRangeQueryHandler
        : IRequestHandler<GetAuditLogsByDateRangeQuery, Result<List<AuditLogDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAuditLogsByDateRangeQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<AuditLogDto>>> Handle(
            GetAuditLogsByDateRangeQuery request,
            CancellationToken cancellationToken)
        {
            var spec = new AuditLogsByDateRangeSpecification(
                request.StartDate,
                request.EndDate);

            var logs = _uow.AuditLogs.Entities
      .Where(spec.Criteria)
      .ToList();
            var dtos = _mapper.Map<List<AuditLogDto>>(logs);

            return Result<List<AuditLogDto>>.Success(dtos);
        }
    }
}