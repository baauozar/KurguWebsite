using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.Partners.Commands
{
    public class UpdatePartnerCommand : UpdatePartnerDto, IRequest<Result<PartnerDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePartnerCommandHandler : IRequestHandler<UpdatePartnerCommand, Result<PartnerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdatePartnerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<PartnerDto>> Handle(UpdatePartnerCommand request, CancellationToken cancellationToken)
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(request.Id);
            if (partner == null) return Result<PartnerDto>.Failure("Partner not found.");

            partner.Update(request.Name, request.LogoPath, request.WebsiteUrl, request.Description, request.Type);
            partner.SetModifiedBy(_currentUserService.UserId ?? "System");

            await _unitOfWork.Partners.UpdateAsync(partner);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Partners, CacheKeys.ActivePartners, CacheKeys.HomePage), cancellationToken);

            return Result<PartnerDto>.Success(_mapper.Map<PartnerDto>(partner));
        }
    }
}