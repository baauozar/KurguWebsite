// src/Core/KurguWebsite.Application/Features/Services/Commands/CreateServiceFeatureCommand.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.ServiceFeaturesDto;
using KurguWebsite.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class CreateServiceFeatureCommand : CreateServiceFeatureDto, IRequest<Result<ServiceFeatureDto>>
    {
        public Guid ServiceId { get; set; }
    }

    public class CreateServiceFeatureCommandHandler : IRequestHandler<CreateServiceFeatureCommand, Result<ServiceFeatureDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateServiceFeatureCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ServiceFeatureDto>> Handle(CreateServiceFeatureCommand request, CancellationToken cancellationToken)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId);
            if (service == null)
            {
                return Result<ServiceFeatureDto>.Failure("Service not found.");
            }

            var serviceFeature = ServiceFeature.Create(
                request.ServiceId,
                request.Title,
                request.Description,
                request.IconClass);

            await _unitOfWork.ServiceFeatures.AddAsync(serviceFeature);
            await _unitOfWork.CommitAsync();

            return Result<ServiceFeatureDto>.Success(_mapper.Map<ServiceFeatureDto>(serviceFeature));
        }
    }
}