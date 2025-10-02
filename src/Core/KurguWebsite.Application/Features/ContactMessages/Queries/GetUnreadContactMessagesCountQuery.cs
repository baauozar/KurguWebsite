// src/Core/KurguWebsite.Application/Features/ContactMessages/Queries/GetUnreadContactMessagesCountQuery.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages.Queries
{
    public class GetUnreadContactMessagesCountQuery : IRequest<Result<int>> { }

    public class GetUnreadContactMessagesCountQueryHandler
        : IRequestHandler<GetUnreadContactMessagesCountQuery, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUnreadContactMessagesCountQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(
            GetUnreadContactMessagesCountQuery request,
            CancellationToken ct)
        {
            var count = await _unitOfWork.ContactMessages.GetUnreadCountAsync();
            return Result<int>.Success(count);
        }
    }
}