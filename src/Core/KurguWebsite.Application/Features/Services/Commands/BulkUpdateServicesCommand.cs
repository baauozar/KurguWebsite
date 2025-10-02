using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class BulkUpdateServicesCommand : IRequest<Result<int>>
    {
        public List<BulkServiceUpdate> Updates { get; set; } = new();
    }

    public class BulkServiceUpdate
    {
        public Guid Id { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class BulkUpdateServicesCommandHandler
        : IRequestHandler<BulkUpdateServicesCommand, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public async Task<Result<int>> Handle(
            BulkUpdateServicesCommand request,
            CancellationToken ct)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                int updated = 0;

                foreach (var update in request.Updates)
                {
                    var service = await _unitOfWork.Services.GetByIdAsync(update.Id);
                    if (service == null) continue;

                    if (update.IsActive.HasValue)
                    {
                        if (update.IsActive.Value) service.Activate();
                        else service.Deactivate();
                    }

                    if (update.IsFeatured.HasValue)
                        service.SetFeatured(update.IsFeatured.Value);

                    if (update.DisplayOrder.HasValue)
                        service.SetDisplayOrder(update.DisplayOrder.Value);

                    await _unitOfWork.Services.UpdateAsync(service);
                    updated++;
                }

                await _unitOfWork.CommitAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);

                await _mediator.Publish(
                    new CacheInvalidationEvent(CacheKeys.Services, CacheKeys.FeaturedServices),
                    ct);

                return Result<int>.Success(updated, $"Updated {updated} services");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return Result<int>.Failure($"Bulk update failed: {ex.Message}");
            }
        }
    }
}