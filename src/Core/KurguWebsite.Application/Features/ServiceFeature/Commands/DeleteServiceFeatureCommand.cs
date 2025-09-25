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

    public DeleteServiceFeatureCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ControlResult> Handle(DeleteServiceFeatureCommand request, CancellationToken cancellationToken)
    {
        var serviceFeature = await _unitOfWork.ServiceFeatures.GetByIdAsync(request.Id);

        if (serviceFeature == null)
        {
            return ControlResult.Failure("Service Feature not found.");
        }

        await _unitOfWork.ServiceFeatures.DeleteAsync(serviceFeature);
        await _unitOfWork.CommitAsync();

        return ControlResult.Success();
    }
}