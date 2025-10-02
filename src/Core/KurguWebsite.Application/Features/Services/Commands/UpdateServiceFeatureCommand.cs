// src/Core/KurguWebsite.Application/Features/Services/Commands/UpdateServiceFeatureCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class UpdateServiceFeatureCommand : IRequest<Result<ServiceFeatureDto>>
    {
        public Guid Id { get; set; }
        public UpdateServiceFeatureDto UpdateServiceFeatureDto { get; set; }
    }

    public class UpdateServiceFeatureCommandHandler : IRequestHandler<UpdateServiceFeatureCommand, Result<ServiceFeatureDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        public UpdateServiceFeatureCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ServiceFeatureDto>> Handle(UpdateServiceFeatureCommand request, CancellationToken cancellationToken)
        {
            var serviceFeature = await _unitOfWork.ServiceFeatures.GetByIdAsync(request.Id);

            if (serviceFeature == null)
            {
                return Result<ServiceFeatureDto>.Failure("Service Feature not found.");
            }

            serviceFeature.Update(
                request.UpdateServiceFeatureDto.Title,
                request.UpdateServiceFeatureDto.Description,
                request.UpdateServiceFeatureDto.IconClass);

            // Track who modified
            serviceFeature.LastModifiedBy = _currentUserService.UserId ?? "System";
            serviceFeature.LastModifiedDate = DateTime.UtcNow;

            await _unitOfWork.ServiceFeatures.UpdateAsync(serviceFeature);
            await _unitOfWork.CommitAsync();

            return Result<ServiceFeatureDto>.Success(_mapper.Map<ServiceFeatureDto>(serviceFeature));
        }
    }
}