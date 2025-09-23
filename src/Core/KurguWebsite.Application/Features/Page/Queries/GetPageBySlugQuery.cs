using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class GetPageBySlugQuery : IRequest<Result<PageDto>>
    {
        public string Slug { get; set; }
    }

    public class GetPageBySlugQueryHandler : IRequestHandler<GetPageBySlugQuery, Result<PageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPageBySlugQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PageDto>> Handle(GetPageBySlugQuery request, CancellationToken cancellationToken)
        {
            var page = await _unitOfWork.Pages.GetBySlugAsync(request.Slug);
            if (page == null) return Result<PageDto>.Failure("Page not found.");

            return Result<PageDto>.Success(_mapper.Map<PageDto>(page));
        }
    }
}