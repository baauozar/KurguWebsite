using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.ProcessSteps.Commands
{
    public class RestoreProcessStepsBatchCommand : IRequest<Result<int>>
    {
        public List<Guid> Ids { get; set; } = new();
    }

    public class RestoreProcessStepsBatchCommandHandler
        : IRequestHandler<RestoreProcessStepsBatchCommand, Result<int>>
    {
        private readonly IUnitOfWork _uow;

        public RestoreProcessStepsBatchCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<int>> Handle(RestoreProcessStepsBatchCommand request, CancellationToken ct)
        {
            int restored = 0;

            foreach (var id in request.Ids.Distinct())
            {
                var entity = await _uow.ProcessSteps.GetByIdIncludingDeletedAsync(id);
                if (entity != null && entity.IsDeleted)
                {
                    await _uow.ProcessSteps.RestoreAsync(entity);
                    restored++;
                }
            }

            await _uow.CommitAsync(ct);
            return Result<int>.Success(restored);
        }
    }
}