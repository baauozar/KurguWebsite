// src/Core/KurguWebsite.Application/Features/Testimonials/Commands/UpdateTestimonialCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Testimonials.Commands
{
    public class UpdateTestimonialCommand : UpdateTestimonialDto,
        IRequest<Result<TestimonialDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateTestimonialCommandHandler
        : IRequestHandler<UpdateTestimonialCommand, Result<TestimonialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateTestimonialCommandHandler> _logger;

        public UpdateTestimonialCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<UpdateTestimonialCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<TestimonialDto>> Handle(
            UpdateTestimonialCommand req,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(req.Id);
                    if (testimonial is null)
                    {
                        return Result<TestimonialDto>.Failure(
                            "Testimonial not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    // FIXED: Now includes ClientImagePath parameter
                    testimonial.Update(
                        clientName: req.ClientName ?? testimonial.ClientName,
                        clientTitle: req.ClientTitle ?? testimonial.ClientTitle,
                        companyName: req.CompanyName ?? testimonial.CompanyName,
                        content: req.Content ?? testimonial.Content,
                        rating: req.Rating ?? testimonial.Rating,
                        clientImagePath: req.ClientImagePath
                    );

                    // Update featured status if provided
                    if (req.IsFeatured.HasValue)
                    {
                        testimonial.SetFeatured(req.IsFeatured.Value);
                    }

                    // Update display order if provided
                    if (req.DisplayOrder.HasValue)
                    {
                        testimonial.SetDisplayOrder(req.DisplayOrder.Value);
                    }

                    // Update active status if provided
                    if (req.IsActive.HasValue)
                    {
                        if (req.IsActive.Value)
                            testimonial.Activate();
                        else
                            testimonial.Deactivate();
                    }

                    testimonial.LastModifiedBy = _currentUserService.UserId ?? "System";
                    testimonial.LastModifiedDate = DateTime.UtcNow;

                    await _unitOfWork.Testimonials.UpdateAsync(testimonial);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Testimonial updated: Id={TestimonialId}", req.Id);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.Testimonials,
                            CacheKeys.ActiveTestimonials,
                            CacheKeys.HomePage),
                        ct);

                    return Result<TestimonialDto>.Success(_mapper.Map<TestimonialDto>(testimonial));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating testimonial: {TestimonialId}", req.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<TestimonialDto>.Failure($"Failed to update testimonial: {ex.Message}");
                }
            }
        }
    }
}