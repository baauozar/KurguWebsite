// src/Core/KurguWebsite.Application/Features/Services/Commands/DeleteServiceFeatureCommand.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using MediatR;

namespace KurguWebsite.Application.Features.Services.Commands;

public class DeleteServiceFeatureCommand : IRequest<ControlResult>
{
    public Guid Id { get; set; }
}

public class DeleteServiceFeatureCommandHandler : IRequestHandler<DeleteServiceFeatureCommand, ControlResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteServiceFeatureCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ControlResult> Handle(DeleteServiceFeatureCommand request, CancellationToken cancellationToken)
    {
        var serviceFeature = await _unitOfWork.ServiceFeatures.GetByIdAsync(request.Id);

        if (serviceFeature == null)
        {
            return ControlResult.Failure("Service Feature not found.");
        }

        // Track who deleted (if using soft delete)
        serviceFeature.SoftDelete(_currentUserService.UserId ?? "System");
        await _unitOfWork.ServiceFeatures.UpdateAsync(serviceFeature);

        await _unitOfWork.CommitAsync();

        return ControlResult.Success();
    }
}