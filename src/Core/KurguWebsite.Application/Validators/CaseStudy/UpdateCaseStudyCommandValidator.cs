using FluentValidation;
using KurguWebsite.Application.Features.CaseStudies.Commands;

namespace KurguWebsite.Application.Validators.CaseStudy
{
    public class CreateCaseStudyMetricCommandValidator : AbstractValidator<CreateCaseStudyMetricCommand>
    {
        public CreateCaseStudyMetricCommandValidator()
        {
            RuleFor(x => x.CaseStudyId)
                .NotEmpty().WithMessage("Case study ID is required");

            RuleFor(x => x.MetricName)
                .NotEmpty().WithMessage("Metric name is required")
                .MaximumLength(100).WithMessage("Metric name cannot exceed 100 characters");

            RuleFor(x => x.MetricValue)
                .NotEmpty().WithMessage("Metric value is required")
                .MaximumLength(100).WithMessage("Metric value cannot exceed 100 characters");

            When(x => !string.IsNullOrEmpty(x.Icon), () =>
            {
                RuleFor(x => x.Icon)
                    .MaximumLength(50).WithMessage("Icon must not exceed 50 characters");
            });
        }
    }
}
