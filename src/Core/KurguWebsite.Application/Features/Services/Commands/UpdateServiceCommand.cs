using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Commands
{
    public class UpdateServiceCommand : UpdateServiceDto, IRequest<Result<ServiceDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, Result<ServiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdateServiceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<ServiceDto>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(request.Id);
            if (service == null) return Result<ServiceDto>.Failure("Service not found.");

            service.Update(request.Title, request.Description, request.IconPath,request.FullDescription,request.ShortDescription,request.Category);
            service.SetModifiedBy(_currentUserService.UserId ?? "System");

            await _unitOfWork.Services.UpdateAsync(service);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.Services, CacheKeys.FeaturedServices, CacheKeys.HomePage), cancellationToken);

            return Result<ServiceDto>.Success(_mapper.Map<ServiceDto>(service));
        }
    }
}