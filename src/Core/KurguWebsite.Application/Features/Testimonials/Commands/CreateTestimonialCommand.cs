// src/Core/KurguWebsite.Application/Features/Testimonials/Commands/CreateTestimonialCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Testimonials.Commands
{
    public class CreateTestimonialCommand : CreateTestimonialDto,
        IRequest<Result<TestimonialDto>>
    { }

    public class CreateTestimonialCommandHandler
        : IRequestHandler<CreateTestimonialCommand, Result<TestimonialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateTestimonialCommandHandler> _logger;

        public CreateTestimonialCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<CreateTestimonialCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<TestimonialDto>> Handle(
            CreateTestimonialCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var maxOrder = await _unitOfWork.Services.GetAllAsync(ct)
             .ContinueWith(t => t.Result.Any() ? t.Result.Max(s => s.DisplayOrder) : 0);
                    var nextDisplayOrder = maxOrder + 1; var testimonial = Testimonial.Create(
                        request.ClientName,
                        request.ClientTitle,
                        request.CompanyName,
                        request.Content,
                        request.ClientImagePath??string.Empty,
                        request.Rating
                    );

                    testimonial.CreatedBy = _currentUserService.UserId ?? "System";
                    testimonial.CreatedDate = DateTime.UtcNow;
                    testimonial.SetDisplayOrder(nextDisplayOrder);
                    await _unitOfWork.Testimonials.AddAsync(testimonial);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Testimonial created: Id={TestimonialId}, ClientName={ClientName}",
                        testimonial.Id, testimonial.ClientName);

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
                    _logger.LogError(ex, "Error creating testimonial");
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<TestimonialDto>.Failure($"Failed to create testimonial: {ex.Message}");
                }
            }
        }
    }
}