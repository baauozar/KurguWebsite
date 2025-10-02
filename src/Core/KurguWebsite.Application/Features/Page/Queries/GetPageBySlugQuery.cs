// src/Core/KurguWebsite.Application/Features/Pages/Queries/GetPageBySlugQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class GetPageBySlugQuery : IRequest<Result<PageDto>>
    {
        public string Slug { get; set; } = string.Empty;
    }

    public class GetPageBySlugQueryHandler
        : IRequestHandler<GetPageBySlugQuery, Result<PageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetPageBySlugQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<PageDto>> Handle(
            GetPageBySlugQuery request,
            CancellationToken ct)
        {
            var cacheKey = string.Format(CacheKeys.PageBySlug, request.Slug);
            var cachedPage = await _cacheService.GetAsync<PageDto>(cacheKey);

            if (cachedPage != null)
            {
                return Result<PageDto>.Success(cachedPage);
            }

            var page = await _unitOfWork.Pages.GetBySlugAsync(request.Slug);
            if (page == null)
            {
                return Result<PageDto>.Failure(
                    "Page not found.",
                    ErrorCodes.EntityNotFound);
            }

            var mappedPage = _mapper.Map<PageDto>(page);
            await _cacheService.SetAsync(cacheKey, mappedPage, TimeSpan.FromMinutes(30));

            return Result<PageDto>.Success(mappedPage);
        }
    }
}