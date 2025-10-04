using FluentValidation;
using KurguWebsite.Application.Features.Partners.Commands;

namespace KurguWebsite.Application.Validators.Partner
{
    public class UpdatePartnerCommandValidator : AbstractValidator<UpdatePartnerCommand>
    {
        public UpdatePartnerCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Partner ID is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Partner name is required")
                .MaximumLength(100).WithMessage("Partner name must not exceed 100 characters");

            RuleFor(x => x.LogoPath)
                .NotEmpty().WithMessage("Logo is required")
                .MaximumLength(500).WithMessage("Logo path must not exceed 500 characters");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid partner type");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");

            When(x => !string.IsNullOrEmpty(x.WebsiteUrl), () =>
            {
                RuleFor(x => x.WebsiteUrl)
                    .Must(BeAValidUrl).WithMessage("Website URL must be a valid URL")
                    .MaximumLength(500).WithMessage("Website URL must not exceed 500 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
            });
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}