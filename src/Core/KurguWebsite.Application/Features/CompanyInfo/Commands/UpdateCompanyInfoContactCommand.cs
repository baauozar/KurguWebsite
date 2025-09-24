using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.ValueObjects;
using MediatR;

namespace KurguWebsite.Application.Features.CompanyInfo.Commands
{
    public class UpdateCompanyInfoContactCommand : UpdateContactInfoDto, IRequest<Result<CompanyInfoDto>> { }

    public class UpdateCompanyInfoContactCommandHandler : IRequestHandler<UpdateCompanyInfoContactCommand, Result<CompanyInfoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdateCompanyInfoContactCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<CompanyInfoDto>> Handle(UpdateCompanyInfoContactCommand request, CancellationToken cancellationToken)
        {
            var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
            if (companyInfo == null) return Result<CompanyInfoDto>.Failure("Company Info not found.");

            // 1. Create the ContactInfo value object first.
            var newContactInfo = ContactInfo.Create(request.SupportEmail, request.SalesEmail, request.Email);

            // 2. Pass the single object to the update method.
            companyInfo.UpdateContactInfo(newContactInfo);

            await _unitOfWork.CompanyInfo.UpdateAsync(companyInfo);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.CompanyInfo, CacheKeys.HomePage, CacheKeys.ContactPage), cancellationToken);

            return Result<CompanyInfoDto>.Success(_mapper.Map<CompanyInfoDto>(companyInfo));
        }
    }
}