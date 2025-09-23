using AutoMapper;
using AutoMapper.QueryableExtensions;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Queries
{
    public class GetPaginatedContactMessagesQuery : IRequest<Result<PaginatedList<ContactMessageDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public QueryParameters Params { get; set; }
    }

    public class GetPaginatedContactMessagesQueryHandler : IRequestHandler<GetPaginatedContactMessagesQuery, Result<PaginatedList<ContactMessageDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPaginatedContactMessagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ContactMessageDto>>> Handle(GetPaginatedContactMessagesQuery request, CancellationToken cancellationToken)
        {
            var messagesQuery = (await _unitOfWork.ContactMessages.GetAsync(
                predicate: null,
                orderBy: q => q.OrderByDescending(m => m.CreatedDate),
                includeString: null,
                disableTracking: true))
                .AsQueryable();

            var paginatedList = await PaginatedList<ContactMessageDto>.CreateAsync(
                messagesQuery.ProjectTo<ContactMessageDto>(_mapper.ConfigurationProvider),
                request.Params.PageNumber,
                request.Params.PageSize);

            return Result<PaginatedList<ContactMessageDto>>.Success(paginatedList);
        }
    }
}