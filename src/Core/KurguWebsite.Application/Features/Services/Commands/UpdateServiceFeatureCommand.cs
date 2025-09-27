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

        public UpdateServiceFeatureCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            await _unitOfWork.ServiceFeatures.UpdateAsync(serviceFeature);
            await _unitOfWork.CommitAsync();

            return Result<ServiceFeatureDto>.Success(_mapper.Map<ServiceFeatureDto>(serviceFeature));
        }
    }
}