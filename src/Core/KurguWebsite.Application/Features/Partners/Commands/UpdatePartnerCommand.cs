// src/Core/KurguWebsite.Application/Features/Partners/Commands/UpdatePartnerCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.Application.Features.Partners.Commands
{
    public class UpdatePartnerCommand : UpdatePartnerDto, IRequest<Result<PartnerDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePartnerCommandHandler 
        : IRequestHandler<UpdatePartnerCommand, Result<PartnerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatePartnerCommandHandler> _logger;

        public UpdatePartnerCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<UpdatePartnerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<PartnerDto>> Handle(
            UpdatePartnerCommand req,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var partner = await _unitOfWork.Partners.GetByIdAsync(req.Id);
                    if (partner is null)
                    {
                        return Result<PartnerDto>.Failure(
                            "Partner not found.",
                            ErrorCodes.EntityNotFound);
                    }

                    partner.Update(
                        req.Name ?? partner.Name,
                        req.LogoPath ?? partner.LogoPath,
                        req.WebsiteUrl ?? partner.WebsiteUrl,
                        req.Description ?? partner.Description,
                        req.Type != default ? req.Type : partner.Type
                    );

                    partner.LastModifiedBy = _currentUserService.UserId ?? "System";
                    partner.LastModifiedDate = DateTime.UtcNow;

                    await _unitOfWork.Partners.UpdateAsync(partner);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation("Partner updated: Id={PartnerId}", req.Id);

                    await _mediator.Publish(
                        new CacheInvalidationEvent(
                            CacheKeys.Partners,
                            CacheKeys.ActivePartners,
                            CacheKeys.HomePage),
                        ct);

                    return Result<PartnerDto>.Success(_mapper.Map<PartnerDto>(partner));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating partner: {PartnerId}", req.Id);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<PartnerDto>.Failure($"Failed to update partner: {ex.Message}");
                }
            }
        }
    }
}