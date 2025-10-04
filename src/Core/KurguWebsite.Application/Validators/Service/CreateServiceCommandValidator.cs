using FluentValidation;
using KurguWebsite.Application.Features.Services.Commands;
using KurguWebsite.Application.Interfaces.Repositories;

namespace KurguWebsite.Application.Validators.Service
{
    public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
    {
        private readonly IServiceUniquenessChecker _checker;

        public CreateServiceCommandValidator(IServiceUniquenessChecker checker)
        {
            _checker = checker;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
                .MustAsync(async (cmd, title, ct) =>
                    !await _checker.TitleExistsAsync(title, null, ct))
                .WithMessage("A service with this title already exists");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Short description is required")
                .MaximumLength(300).WithMessage("Short description must not exceed 300 characters");

            RuleFor(x => x.IconPath)
                .NotEmpty().WithMessage("Icon path is required")
                .MaximumLength(500).WithMessage("Icon path must not exceed 500 characters");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid service category");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");

            When(x => !string.IsNullOrEmpty(x.FullDescription), () =>
            {
                RuleFor(x => x.FullDescription)
                    .MaximumLength(5000).WithMessage("Full description must not exceed 5000 characters");
            });

            When(x => !string.IsNullOrEmpty(x.IconClass), () =>
            {
                RuleFor(x => x.IconClass)
                    .MaximumLength(50).WithMessage("Icon class must not exceed 50 characters");
            });

            When(x => !string.IsNullOrEmpty(x.MetaTitle), () =>
            {
                RuleFor(x => x.MetaTitle)
                    .MaximumLength(100).WithMessage("Meta title must not exceed 100 characters");
            });

            When(x => !string.IsNullOrEmpty(x.MetaDescription), () =>
            {
                RuleFor(x => x.MetaDescription)
                    .MaximumLength(200).WithMessage("Meta description must not exceed 200 characters");
            });

            When(x => !string.IsNullOrEmpty(x.MetaKeywords), () =>
            {
                RuleFor(x => x.MetaKeywords)
                    .MaximumLength(300).WithMessage("Meta keywords must not exceed 300 characters");
            });

            When(x => x.Features != null && x.Features.Any(), () =>
            {
                RuleFor(x => x.Features)
                    .Must(f => f.Count <= 50).WithMessage("Cannot have more than 50 features");

                RuleForEach(x => x.Features).ChildRules(feature =>
                {
                    feature.RuleFor(f => f.Title)
                        .NotEmpty().WithMessage("Feature title is required")
                        .MaximumLength(100).WithMessage("Feature title must not exceed 100 characters");

                    feature.RuleFor(f => f.Description)
                        .NotEmpty().WithMessage("Feature description is required")
                        .MaximumLength(500).WithMessage("Feature description must not exceed 500 characters");
                });
            });
        }
    }
}
