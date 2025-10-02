// src/Core/KurguWebsite.Application/Features/Pages/Queries/GetPageByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Queries
{
    public class GetPageByIdQuery : IRequest<Result<PageDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetPageByIdQueryHandler
        : IRequestHandler<GetPageByIdQuery, Result<PageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPageByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PageDto>> Handle(
            GetPageByIdQuery request,
            CancellationToken ct)
        {
            var page = await _unitOfWork.Pages.GetByIdAsync(request.Id);
            if (page == null)
            {
                return Result<PageDto>.Failure(
                    "Page not found.",
                    ErrorCodes.EntityNotFound);
            }

            return Result<PageDto>.Success(_mapper.Map<PageDto>(page));
        }
    }
}