// src/Core/KurguWebsite.Application/Features/ContactMessages/Queries/SearchContactMessagesQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Queries
{
    public class SearchContactMessagesQuery : IRequest<Result<PaginatedList<ContactMessageDto>>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsReplied { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SearchContactMessagesQueryHandler
        : IRequestHandler<SearchContactMessagesQuery, Result<PaginatedList<ContactMessageDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SearchContactMessagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ContactMessageDto>>> Handle(
            SearchContactMessagesQuery request,
            CancellationToken ct)
        {
            var spec = new MessageSearchSpecification(
                searchTerm: request.SearchTerm,
                isRead: request.IsRead,
                isReplied: request.IsReplied,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            var messages = await _unitOfWork.ContactMessages.ListAsync(spec, ct);

            var countSpec = new MessageSearchSpecification(
                searchTerm: request.SearchTerm,
                isRead: request.IsRead,
                isReplied: request.IsReplied,
                pageNumber: 1,
                pageSize: int.MaxValue);
            var totalCount = await _unitOfWork.ContactMessages.CountAsync(countSpec, ct);

            var mappedMessages = _mapper.Map<List<ContactMessageDto>>(messages);

            var paginatedList = new PaginatedList<ContactMessageDto>(
                mappedMessages,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedList<ContactMessageDto>>.Success(paginatedList);
        }
    }
}