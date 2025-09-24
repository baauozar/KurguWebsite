using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.ValueObjects;
using MediatR;

namespace KurguWebsite.Application.Features.CompanyInfo.Commands
{
    public class UpdateCompanyInfoSocialMediaCommand : UpdateSocialMediaDto, IRequest<Result<CompanyInfoDto>> { }

    public class UpdateCompanyInfoSocialMediaCommandHandler : IRequestHandler<UpdateCompanyInfoSocialMediaCommand, Result<CompanyInfoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdateCompanyInfoSocialMediaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<CompanyInfoDto>> Handle(UpdateCompanyInfoSocialMediaCommand request, CancellationToken cancellationToken)
        {
            var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
            if (companyInfo == null) return Result<CompanyInfoDto>.Failure("Company Info not found.");

            var socialMedia = SocialMediaLinks.Create(request.Facebook, request.Twitter, request.LinkedIn, request.Instagram, request.YouTube);
            companyInfo.UpdateSocialMedia(socialMedia);

            // REMOVED: companyInfo.SetModifiedBy(_currentUserService.UserId ?? "System");

            await _unitOfWork.CompanyInfo.UpdateAsync(companyInfo);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.CompanyInfo, CacheKeys.HomePage, CacheKeys.AboutPage), cancellationToken);

            return Result<CompanyInfoDto>.Success(_mapper.Map<CompanyInfoDto>(companyInfo));
        }
    }
}