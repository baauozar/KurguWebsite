using FluentValidation;
using KurguWebsite.Application.DTOs.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.Partner
{
    public class CreatePartnerDtoValidator : AbstractValidator<CreatePartnerDto>
    {
        public CreatePartnerDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Partner name is required")
                .MaximumLength(100).WithMessage("Partner name must not exceed 100 characters");

            RuleFor(x => x.LogoPath)
                .NotEmpty().WithMessage("Logo is required");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid partner type");

            When(x => !string.IsNullOrEmpty(x.WebsiteUrl), () =>
            {
                RuleFor(x => x.WebsiteUrl)
                    .Must(BeAValidUrl).WithMessage("Website URL must be a valid URL");
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