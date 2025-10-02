using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.Partners.Commands
{
    public class CreatePartnerCommand : CreatePartnerDto, IRequest<Result<PartnerDto>> { }

    public class CreatePartnerCommandHandler : IRequestHandler<CreatePartnerCommand, Result<PartnerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public CreatePartnerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<PartnerDto>> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
        {
            var partner = Partner.Create(request.Name, request.LogoPath, request.Type);
            partner.Update(request.Name, request.LogoPath, request.WebsiteUrl, request.Description, request.Type);

            // Track who created
            partner.CreatedBy = _currentUserService.UserId ?? "System";
            partner.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.Partners.AddAsync(partner);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Partners, CacheKeys.ActivePartners, CacheKeys.HomePage), cancellationToken);

            return Result<PartnerDto>.Success(_mapper.Map<PartnerDto>(partner));
        }
    }
}