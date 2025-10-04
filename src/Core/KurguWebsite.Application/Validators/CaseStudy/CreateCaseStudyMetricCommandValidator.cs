using FluentValidation;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.Features.CaseStudies.Commands;

namespace KurguWebsite.Application.Validators.CaseStudy
{
    public class CreateCaseStudyMetricDtoValidator : AbstractValidator<CreateCaseStudyMetricCommand>
    {
        public CreateCaseStudyMetricDtoValidator()
        {
            RuleFor(x => x.MetricName)
                .NotEmpty().WithMessage("Metric name is required.")
                .MaximumLength(100).WithMessage("Metric name cannot exceed 100 characters.");

            RuleFor(x => x.MetricValue)
                .NotEmpty().WithMessage("Metric value is required.")
                .MaximumLength(100).WithMessage("Metric value cannot exceed 100 characters.");
        }
    }
}