
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Domain.Events;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Features.CaseStudies.Commands
{
    // 1. Change the return type to your non-generic Result
    public class DeleteCaseStudyCommand : IRequest<ControlResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCaseStudyCommandHandler : IRequestHandler<DeleteCaseStudyCommand, ControlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public DeleteCaseStudyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        // 2. Change the method's return type to Task<Result>
        public async Task<ControlResult> Handle(DeleteCaseStudyCommand request, CancellationToken cancellationToken)
        {
            var caseStudy = await _unitOfWork.CaseStudies.GetByIdAsync(request.Id);

            // 3. Use your Result.Failure method
            if (caseStudy == null) return ControlResult.Failure("Case Study not found.");

            caseStudy.SoftDelete(_currentUserService.UserId ?? "System");
            await _unitOfWork.CaseStudies.UpdateAsync(caseStudy);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _mediator.Publish(new CacheInvalidationEvent(CacheKeys.CaseStudies, CacheKeys.FeaturedCaseStudies), cancellationToken);

            // 4. Use your Result.Success method
            return ControlResult.Success();
        }
    }
}