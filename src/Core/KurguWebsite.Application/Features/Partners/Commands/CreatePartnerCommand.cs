// src/Core/KurguWebsite.Application/Features/Partners/Commands/CreatePartnerCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace KurguWebsite.Application.Features.Partners.Commands
{
    public class CreatePartnerCommand : CreatePartnerDto, IRequest<Result<PartnerDto>> { }

    public class CreatePartnerCommandHandler
        : IRequestHandler<CreatePartnerCommand, Result<PartnerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<CreatePartnerCommandHandler> _logger;

        public CreatePartnerCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator,
            ILogger<CreatePartnerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<PartnerDto>> Handle(
            CreatePartnerCommand request,
            CancellationToken ct)
        {
            using (await _unitOfWork.BeginTransactionAsync(ct))
            {
                try
                {
                    var maxOrder = await _unitOfWork.Services.GetAllAsync(ct)
             .ContinueWith(t => t.Result.Any() ? t.Result.Max(s => s.DisplayOrder) : 0);
                    var nextDisplayOrder = maxOrder + 1; var partner = Partner.Create(
                request.Name,
            request.LogoPath,
            request.WebsiteUrl,
            request.Description,
            request.Type);

                    partner.CreatedBy = _currentUserService.UserId ?? "System";
                    partner.CreatedDate = DateTime.UtcNow;
                    partner.SetDisplayOrder(nextDisplayOrder);
                    await _unitOfWork.Partners.AddAsync(partner);
                    await _unitOfWork.CommitAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    _logger.LogInformation(
                        "Partner created: Id={PartnerId}, Name={Name}",
                        partner.Id, partner.Name);

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
                    _logger.LogError(ex, "Error creating partner: {Name}", request.Name);
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result<PartnerDto>.Failure($"Failed to create partner: {ex.Message}");
                }
            }
        }
    }
}