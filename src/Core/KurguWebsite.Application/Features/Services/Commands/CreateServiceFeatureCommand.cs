using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Entities;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class CreateServiceFeatureCommand : IRequest<Result<CreateServiceFeatureDto>>
    {
        public Guid ServiceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CreateServiceFeatureCommandHandler
        : IRequestHandler<CreateServiceFeatureCommand, Result<CreateServiceFeatureDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateServiceFeatureCommandHandler(IUnitOfWork uow, IMapper mapper)
        {
            _unitOfWork = uow;
            _mapper = mapper;
        }

        public async Task<Result<CreateServiceFeatureDto>> Handle(CreateServiceFeatureCommand request, CancellationToken ct)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId);
            if (service == null)
                return Result<CreateServiceFeatureDto>.Failure("Service not found.");

            var entity = ServiceFeature.Create(
                request.ServiceId,
                request.Title,
                request.Description,
                request.IconClass);

            if (request.DisplayOrder > 0)
            {
                typeof(ServiceFeature).GetProperty("DisplayOrder")!
                    .SetValue(entity, request.DisplayOrder);
            }

            await _unitOfWork.ServiceFeatures.AddAsync(entity);
            await _unitOfWork.CommitAsync();

            // Map to the SAME type you return
            var dto = _mapper.Map<CreateServiceFeatureDto>(entity);
            return Result<CreateServiceFeatureDto>.Success(dto);
        }
    }
}
