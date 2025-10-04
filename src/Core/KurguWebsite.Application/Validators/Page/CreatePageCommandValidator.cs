using FluentValidation;
using KurguWebsite.Application.Features.Pages.Commands;

namespace KurguWebsite.Application.Validators.Page
{
    public class CreatePageCommandValidator : AbstractValidator<CreatePageCommand>
    {
        public CreatePageCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Page title is required")
                .MaximumLength(200).WithMessage("Page title must not exceed 200 characters");

            RuleFor(x => x.PageType)
                .IsInEnum().WithMessage("Invalid page type");

            When(x => !string.IsNullOrEmpty(x.Content), () =>
            {
                RuleFor(x => x.Content)
                    .MaximumLength(50000).WithMessage("Page content must not exceed 50000 characters");
            });

            // Hero Section Validations
            When(x => !string.IsNullOrEmpty(x.HeroTitle), () =>
            {
                RuleFor(x => x.HeroTitle)
                    .MaximumLength(200).WithMessage("Hero title must not exceed 200 characters");
            });

            When(x => !string.IsNullOrEmpty(x.HeroSubtitle), () =>
            {
                RuleFor(x => x.HeroSubtitle)
                    .MaximumLength(300).WithMessage("Hero subtitle must not exceed 300 characters");
            });

            When(x => !string.IsNullOrEmpty(x.HeroDescription), () =>
            {
                RuleFor(x => x.HeroDescription)
                    .MaximumLength(1000).WithMessage("Hero description must not exceed 1000 characters");
            });

            When(x => !string.IsNullOrEmpty(x.HeroBackgroundImage), () =>
            {
                RuleFor(x => x.HeroBackgroundImage)
                    .MaximumLength(500).WithMessage("Hero background image path must not exceed 500 characters");
            });

            When(x => !string.IsNullOrEmpty(x.HeroButtonText), () =>
            {
                RuleFor(x => x.HeroButtonText)
                    .MaximumLength(100).WithMessage("Hero button text must not exceed 100 characters");
            });

            When(x => !string.IsNullOrEmpty(x.HeroButtonUrl), () =>
            {
                RuleFor(x => x.HeroButtonUrl)
                    .MaximumLength(500).WithMessage("Hero button URL must not exceed 500 characters");
            });

            // SEO Validations
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
        }
    }
}