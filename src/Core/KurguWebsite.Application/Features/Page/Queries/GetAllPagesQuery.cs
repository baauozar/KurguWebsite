// src/Core/KurguWebsite.Application/Features/Pages/Queries/GetAllPagesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class GetAllPagesQuery : IRequest<Result<List<PageDto>>> { }

    public class GetAllPagesQueryHandler
        : IRequestHandler<GetAllPagesQuery, Result<List<PageDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetAllPagesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<List<PageDto>>> Handle(
            GetAllPagesQuery request,
            CancellationToken ct)
        {
            var cachedPages = await _cacheService.GetAsync<List<PageDto>>(CacheKeys.Pages);
            if (cachedPages != null)
            {
                return Result<List<PageDto>>.Success(cachedPages);
            }

            var spec = new ActivePagesSpecification();
            var pages = await _unitOfWork.Pages.ListAsync(spec, ct);
            var mappedPages = _mapper.Map<List<PageDto>>(pages);

            await _cacheService.SetAsync(CacheKeys.Pages, mappedPages, TimeSpan.FromMinutes(30));

            return Result<List<PageDto>>.Success(mappedPages);
        }
    }
}