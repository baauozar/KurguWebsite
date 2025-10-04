using FluentValidation;
using KurguWebsite.Application.Features.ProcessSteps.Commands;

namespace KurguWebsite.Application.Validators.ProcessStep
{
    public class UpdateProcessStepCommandValidator : AbstractValidator<UpdateProcessStepCommand>
    {
        public UpdateProcessStepCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Process step ID is required");

            When(x => x.StepNumber.HasValue, () =>
            {
                RuleFor(x => x.StepNumber)
                    .GreaterThan(0).WithMessage("Step number must be greater than 0")
                    .LessThanOrEqualTo(100).WithMessage("Step number must not exceed 100");
            });

            When(x => !string.IsNullOrEmpty(x.Title), () =>
            {
                RuleFor(x => x.Title)
                    .NotEmpty().WithMessage("Process step title cannot be empty when provided")
                    .MaximumLength(200).WithMessage("Process step title must not exceed 200 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .NotEmpty().WithMessage("Process step description cannot be empty when provided")
                    .MaximumLength(1000).WithMessage("Process step description must not exceed 1000 characters");
            });

            When(x => x.DisplayOrder.HasValue, () =>
            {
                RuleFor(x => x.DisplayOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
            });

            When(x => !string.IsNullOrEmpty(x.IconClass), () =>
            {
                RuleFor(x => x.IconClass)
                    .MaximumLength(50).WithMessage("Icon class must not exceed 50 characters");
            });
        }
    }
}