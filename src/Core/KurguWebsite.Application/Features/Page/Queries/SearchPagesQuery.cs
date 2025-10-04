// src/Core/KurguWebsite.Application/Features/Pages/Queries/SearchPagesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class SearchPagesQuery : IRequest<Result<List<PageDto>>>
    {
        public string? SearchTerm { get; set; }
        public PageType? PageType { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }

    public class SearchPagesQueryHandler
        : IRequestHandler<SearchPagesQuery, Result<List<PageDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SearchPagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<PageDto>>> Handle(
            SearchPagesQuery request,
            CancellationToken ct)
        {
            var spec = new PageSearchSpecification(
                searchTerm: request.SearchTerm,
                pageType: request.PageType,
                includeInactive: request.IncludeInactive);

            var pages = await _unitOfWork.Pages.ListAsync(spec, ct);
            var mappedPages = _mapper.Map<List<PageDto>>(pages);

            return Result<List<PageDto>>.Success(mappedPages);
        }
    }
}