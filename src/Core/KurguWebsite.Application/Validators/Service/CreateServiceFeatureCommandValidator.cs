using FluentValidation;
using KurguWebsite.Application.Features.Services.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.Service
{
    public class CreateServiceFeatureCommandValidator : AbstractValidator<CreateServiceFeatureCommand>
    {
        public CreateServiceFeatureCommandValidator()
        {
            RuleFor(x => x.ServiceId)
                .NotEmpty().WithMessage("Service ID is required");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Feature title is required")
                .MaximumLength(100).WithMessage("Feature title must not exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Feature description is required")
                .MaximumLength(500).WithMessage("Feature description must not exceed 500 characters");

            When(x => !string.IsNullOrEmpty(x.IconClass), () =>
            {
                RuleFor(x => x.IconClass)
                    .MaximumLength(50).WithMessage("Icon class must not exceed 50 characters");
            });

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
        }
    }
}
