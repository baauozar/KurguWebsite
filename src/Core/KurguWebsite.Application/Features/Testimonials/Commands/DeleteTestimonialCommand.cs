// src/Core/KurguWebsite.Application/Features/Testimonials/Commands/DeleteTestimonialCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Testimonials.Commands
{
    public class DeleteTestimonialCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteTestimonialCommandHandler
        : IRequestHandler<DeleteTestimonialCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteTestimonialCommandHandler> _logger;

        public DeleteTestimonialCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<DeleteTestimonialCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ControlResult> Handle(
            DeleteTestimonialCommand request,
            CancellationToken ct)
        {
            try
            {
                var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(request.Id);
                if (testimonial == null)
                {
                    return ControlResult.Failure("Testimonial not found.");
                }

                testimonial.SoftDelete(_currentUserService.UserId ?? "System");
                await _unitOfWork.Testimonials.UpdateAsync(testimonial);

                var remainingTestimonials = (await _unitOfWork.Testimonials.GetAllAsync())
                                            .Where(t => t.IsActive && t.Id != request.Id)
                                            .OrderBy(t => t.DisplayOrder)
                                            .ToList();

                remainingTestimonials.Reorder();

                foreach (var item in remainingTestimonials)
                {
                    await _unitOfWork.Testimonials.UpdateAsync(item);
                }

                await _unitOfWork.CommitAsync(ct);

                _logger.LogInformation("Testimonial soft-deleted and reordered: Id={TestimonialId}", request.Id);

                await _mediator.Publish(
                    new CacheInvalidationEvent(
                        CacheKeys.Testimonials,
                        CacheKeys.ActiveTestimonials,
                        CacheKeys.HomePage),
                    ct);

                return ControlResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting testimonial: {TestimonialId}", request.Id);
                return ControlResult.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}