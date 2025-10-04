using FluentValidation;
using KurguWebsite.Application.Features.ProcessSteps.Commands;

namespace KurguWebsite.Application.Validators.ProcessStep
{
    public class CreateProcessStepCommandValidator : AbstractValidator<CreateProcessStepCommand>
    {
        public CreateProcessStepCommandValidator()
        {
            RuleFor(x => x.StepNumber)
                .GreaterThan(0).WithMessage("Step number must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Step number must not exceed 100");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Process step title is required")
                .MaximumLength(200).WithMessage("Process step title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Process step description is required")
                .MaximumLength(1000).WithMessage("Process step description must not exceed 1000 characters");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");

            When(x => !string.IsNullOrEmpty(x.IconClass), () =>
            {
                RuleFor(x => x.IconClass)
                    .MaximumLength(50).WithMessage("Icon class must not exceed 50 characters");
            });
        }
    }
}