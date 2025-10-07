using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.CompanyInfo.Commands
{
    public class UpdateCompanySectionImagesCommand : UpdateCompanySectionImagesDto, IRequest<Result<CompanyInfoDto>> { }

    public class UpdateCompanySectionImagesCommandHandler : IRequestHandler<UpdateCompanySectionImagesCommand, Result<CompanyInfoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdateCompanySectionImagesCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<CompanyInfoDto>> Handle(
            UpdateCompanySectionImagesCommand request,
            CancellationToken cancellationToken)
        {
            var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
            if (companyInfo == null)
                return Result<CompanyInfoDto>.Failure("Company Info not found.");

            companyInfo.UpdateSectionImages(
                request.MissionImagePath,
                request.VisionImagePath
          
            );

            companyInfo.LastModifiedBy = _currentUserService.UserId ?? "System";
            companyInfo.LastModifiedDate = DateTime.UtcNow;

            await _unitOfWork.CompanyInfo.UpdateAsync(companyInfo);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(
                new CacheInvalidationEvent(
                    CacheKeys.CompanyInfo,
                    CacheKeys.AboutPage),
                cancellationToken);

            return Result<CompanyInfoDto>.Success(_mapper.Map<CompanyInfoDto>(companyInfo));
        }
    }
}