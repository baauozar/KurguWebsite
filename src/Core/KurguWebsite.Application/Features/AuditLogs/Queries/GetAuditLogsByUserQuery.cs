// src/Core/KurguWebsite.Application/Features/AuditLogs/Queries/GetAuditLogsByUserQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.AuditLogs.Queries
{
    public class GetAuditLogsByUserQuery : IRequest<Result<List<AuditLogDto>>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class GetAuditLogsByUserQueryHandler
        : IRequestHandler<GetAuditLogsByUserQuery, Result<List<AuditLogDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAuditLogsByUserQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<AuditLogDto>>> Handle(
            GetAuditLogsByUserQuery request,
            CancellationToken cancellationToken)
        {
            var logs = await _uow.AuditLogs.GetLogsByUserIdAsync(request.UserId);
            var dtos = _mapper.Map<List<AuditLogDto>>(logs);

            return Result<List<AuditLogDto>>.Success(dtos);
        }
    }
}