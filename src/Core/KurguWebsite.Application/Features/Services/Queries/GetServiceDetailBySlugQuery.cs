using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Queries
{
    public class GetServiceDetailBySlugQuery : IRequest<Result<ServiceDetailDto>>
    {
        public string Slug { get; set; }
    }

    public class GetServiceDetailBySlugQueryHandler : IRequestHandler<GetServiceDetailBySlugQuery, Result<ServiceDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetServiceDetailBySlugQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ServiceDetailDto>> Handle(GetServiceDetailBySlugQuery request, CancellationToken cancellationToken)
        {
            // Assuming your repository has a method to get service with its features by slug
            var service = await _unitOfWork.Services.GetBySlugWithFeaturesAsync(request.Slug);
            if (service == null) return Result<ServiceDetailDto>.Failure("Service not found.");

            return Result<ServiceDetailDto>.Success(_mapper.Map<ServiceDetailDto>(service));
        }
    }
}