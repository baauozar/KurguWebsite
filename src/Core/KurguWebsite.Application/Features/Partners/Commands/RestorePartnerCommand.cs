// src/Core/KurguWebsite.Application/Features/Partners/Commands/RestorePartnerCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Partners.Commands
{
    public class RestorePartnerCommand : IRequest<Result<PartnerDto>>
    {
        public Guid Id { get; set; }
    }

    public class RestorePartnerCommandHandler
        : IRequestHandler<RestorePartnerCommand, Result<PartnerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RestorePartnerCommandHandler> _logger;

        public RestorePartnerCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<RestorePartnerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<PartnerDto>> Handle(
            RestorePartnerCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var entity = await _unitOfWork.Partners.GetByIdIncludingDeletedAsync(request.Id);
                    if (entity is null)
                    {
                        return Result<PartnerDto>.Failure(
                            "Partner not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    if (entity.IsDeleted)
                    {
                        await _unitOfWork.Partners.RestoreAsync(entity);
                        entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                        entity.LastModifiedDate = DateTime.UtcNow;

                        await _unitOfWork.CommitAsync(ct);
                        await _unitOfWork.CommitTransactionAsync(ct);

                        _logger.LogInformation("Partner restored: Id={PartnerId}", request.Id);

                        await _mediator.Publish(
                            new CacheInvalidationEvent(
                                CacheKeys.Partners,
                                CacheKeys.ActivePartners,
                                CacheKeys.HomePage),
                            ct);
                    }

                    return Result<PartnerDto>.Success(_mapper.Map<PartnerDto>(entity));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error restoring partner: {PartnerId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<PartnerDto>.Failure($"Failed to restore partner: {ex.Message}");
                }
            }
        }
    }
}