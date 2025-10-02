using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.Testimonials.Commands
{
    public class RestoreTestimonialBatchCommand : IRequest<Result<int>>
    {
        public List<Guid> Ids { get; set; } = new();
    }

    public class RestoreTestimonialBatchCommandHandler
        : IRequestHandler<RestoreTestimonialBatchCommand, Result<int>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService; // ADD

        public RestoreTestimonialBatchCommandHandler(IUnitOfWork uow, ICurrentUserService currentUserService)
        {
            _uow = uow;
            _currentUserService = currentUserService;
        }

        public async Task<Result<int>> Handle(RestoreTestimonialBatchCommand request, CancellationToken ct)
        {
            int restored = 0;

            foreach (var id in request.Ids.Distinct())
            {
                var entity = await _uow.Testimonials.GetByIdIncludingDeletedAsync(id);
                if (entity != null && entity.IsDeleted)
                {
                    await _uow.Testimonials.RestoreAsync(entity);

                    // FIX: Track who restored
                    entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                    entity.LastModifiedDate = DateTime.UtcNow;

                    restored++;
                }
            }

            await _uow.CommitAsync(ct);
            return Result<int>.Success(restored);
        }
    }
}