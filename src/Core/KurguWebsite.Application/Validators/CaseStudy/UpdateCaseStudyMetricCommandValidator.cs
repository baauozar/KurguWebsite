using FluentValidation;
using KurguWebsite.Application.Features.CaseStudies.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.CaseStudy
{
    public class UpdateCaseStudyMetricCommandValidator : AbstractValidator<UpdateCaseStudyMetricCommand>
    {
        public UpdateCaseStudyMetricCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Metric ID is required");

            RuleFor(x => x.UpdateCaseStudyMetricDto)
                .NotNull().WithMessage("Update data is required");

            When(x => x.UpdateCaseStudyMetricDto != null, () =>
            {
                RuleFor(x => x.UpdateCaseStudyMetricDto.MetricName)
                    .NotEmpty().WithMessage("Metric name is required")
                    .MaximumLength(100).WithMessage("Metric name cannot exceed 100 characters");

                RuleFor(x => x.UpdateCaseStudyMetricDto.MetricValue)
                    .NotEmpty().WithMessage("Metric value is required")
                    .MaximumLength(100).WithMessage("Metric value cannot exceed 100 characters");

                When(x => !string.IsNullOrEmpty(x.UpdateCaseStudyMetricDto.Icon), () =>
                {
                    RuleFor(x => x.UpdateCaseStudyMetricDto.Icon)
                        .MaximumLength(50).WithMessage("Icon must not exceed 50 characters");
                });
            });
        }
    }
}

