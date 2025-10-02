using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.ValueObjects;
using MediatR;

namespace KurguWebsite.Application.Features.CompanyInfo.Commands
{
    public class UpdateCompanyInfoAddressCommand : UpdateAddressDto, IRequest<Result<CompanyInfoDto>> { }

    public class UpdateCompanyInfoAddressCommandHandler : IRequestHandler<UpdateCompanyInfoAddressCommand, Result<CompanyInfoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdateCompanyInfoAddressCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<CompanyInfoDto>> Handle(UpdateCompanyInfoAddressCommand request, CancellationToken cancellationToken)
        {
            var companyInfo = await _unitOfWork.CompanyInfo.GetCompanyInfoAsync();
            if (companyInfo == null) return Result<CompanyInfoDto>.Failure("Company Info not found.");

            var address = Address.Create(request.Street ?? string.Empty, request.Suite, request.City ?? string.Empty, request.State ?? string.Empty, request.PostalCode ?? string.Empty, request.Country ?? string.Empty);
            companyInfo.UpdateAddress(address);

            // Track who modified
            companyInfo.LastModifiedBy = _currentUserService.UserId ?? "System";
            companyInfo.LastModifiedDate = DateTime.UtcNow;

            await _unitOfWork.CompanyInfo.UpdateAsync(companyInfo);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.CompanyInfo, CacheKeys.HomePage, CacheKeys.ContactPage), cancellationToken);

            return Result<CompanyInfoDto>.Success(_mapper.Map<CompanyInfoDto>(companyInfo));
        }
    }
}