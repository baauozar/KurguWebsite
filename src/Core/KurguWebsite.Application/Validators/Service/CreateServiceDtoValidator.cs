using FluentValidation;
using KurguWebsite.Application.DTOs.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.Service
{
    public class CreateServiceDtoValidator : AbstractValidator<CreateServiceDto>
    {
        public CreateServiceDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Short description is required")
                .MaximumLength(300).WithMessage("Short description must not exceed 300 characters");

            RuleFor(x => x.IconPath)
                .NotEmpty().WithMessage("Icon is required");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid service category");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");

            When(x => x.Features != null && x.Features.Any(), () =>
            {
                RuleForEach(x => x.Features).SetValidator(new CreateServiceFeatureDtoValidator());
            });
        }
    }
    public class CreateServiceFeatureDtoValidator : AbstractValidator<CreateServiceFeatureDto>
    {
        public CreateServiceFeatureDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Feature title is required")
                .MaximumLength(100).WithMessage("Feature title must not exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Feature description is required")
                .MaximumLength(500).WithMessage("Feature description must not exceed 500 characters");
        }
    }
}

