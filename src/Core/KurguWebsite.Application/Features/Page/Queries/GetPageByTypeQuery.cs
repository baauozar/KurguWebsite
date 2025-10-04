// src/Core/KurguWebsite.Application/Features/Pages/Queries/GetPageByTypeQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class GetPageByTypeQuery : IRequest<Result<PageDto>>
    {
        public PageType PageType { get; set; }
    }

    public class GetPageByTypeQueryHandler : IRequestHandler<GetPageByTypeQuery, Result<PageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPageByTypeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PageDto>> Handle(GetPageByTypeQuery request, CancellationToken cancellationToken)
        {
            var spec = new PageByTypeSpecification(request.PageType);
            var page = await _unitOfWork.Pages.GetBySpecAsync(spec, cancellationToken);

            if (page == null)
                return Result<PageDto>.Failure($"Page of type '{request.PageType}' not found.");

            return Result<PageDto>.Success(_mapper.Map<PageDto>(page));
        }
    }
}