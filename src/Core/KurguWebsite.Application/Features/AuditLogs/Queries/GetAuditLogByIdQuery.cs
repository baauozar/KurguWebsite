// src/Core/KurguWebsite.Application/Features/AuditLogs/Queries/GetAuditLogByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Audit;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.AuditLogs.Queries
{
    public class GetAuditLogByIdQuery : IRequest<Result<AuditLogDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetAuditLogByIdQueryHandler
        : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAuditLogByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<AuditLogDto>> Handle(
            GetAuditLogByIdQuery request,
            CancellationToken cancellationToken)
        {
            var log = await _uow.AuditLogs.GetByIdAsync(request.Id, cancellationToken);

            if (log == null)
            {
                return Result<AuditLogDto>.Failure("Audit log not found");
            }

            var dto = _mapper.Map<AuditLogDto>(log);
            return Result<AuditLogDto>.Success(dto);
        }
    }
}