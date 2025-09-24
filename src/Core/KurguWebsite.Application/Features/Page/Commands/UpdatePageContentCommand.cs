using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Domain.Events;
using MediatR;

namespace KurguWebsite.Application.Features.Pages.Commands
{
    public class UpdatePageContentCommand : UpdatePageDto, IRequest<Result<PageDto>>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePageContentCommandHandler : IRequestHandler<UpdatePageContentCommand, Result<PageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdatePageContentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<PageDto>> Handle(UpdatePageContentCommand request, CancellationToken cancellationToken)
        {
            var page = await _unitOfWork.Pages.GetByIdAsync(request.Id);
            if (page == null) return Result<PageDto>.Failure("Page not found.");

            page.UpdateContent(request.Content);
            page.UpdateSeo(request.MetaTitle, request.MetaDescription, request.MetaKeywords);

     

            await _unitOfWork.Pages.UpdateAsync(page);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.HomePage, CacheKeys.AboutPage, CacheKeys.ServicesPage, CacheKeys.ContactPage), cancellationToken);

            return Result<PageDto>.Success(_mapper.Map<PageDto>(page));
        }
    }
}