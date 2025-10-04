using FluentValidation;
using KurguWebsite.Application.Features.Services.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.Service
{
    public class UpdateServiceFeatureCommandValidator : AbstractValidator<UpdateServiceFeatureCommand>
    {
        public UpdateServiceFeatureCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Feature ID is required");

            RuleFor(x => x.UpdateServiceFeatureDto)
                .NotNull().WithMessage("Update data is required");

            When(x => x.UpdateServiceFeatureDto != null, () =>
            {
                RuleFor(x => x.UpdateServiceFeatureDto.Title)
                    .NotEmpty().WithMessage("Feature title is required")
                    .MaximumLength(100).WithMessage("Feature title must not exceed 100 characters");

                RuleFor(x => x.UpdateServiceFeatureDto.Description)
                    .NotEmpty().WithMessage("Feature description is required")
                    .MaximumLength(500).WithMessage("Feature description must not exceed 500 characters");

                When(x => !string.IsNullOrEmpty(x.UpdateServiceFeatureDto.IconClass), () =>
                {
                    RuleFor(x => x.UpdateServiceFeatureDto.IconClass)
                        .MaximumLength(50).WithMessage("Icon class must not exceed 50 characters");
                });
            });
        }
    }
}