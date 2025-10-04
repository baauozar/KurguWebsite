using FluentValidation;
using KurguWebsite.Application.Features.CompanyInfo.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.CompanyInfo
{
    public class UpdateCompanyInfoSocialMediaCommandValidator : AbstractValidator<UpdateCompanyInfoSocialMediaCommand>
    {
        public UpdateCompanyInfoSocialMediaCommandValidator()
        {
            When(x => !string.IsNullOrEmpty(x.Facebook), () =>
            {
                RuleFor(x => x.Facebook)
                    .Must(BeAValidUrl).WithMessage("Facebook URL must be a valid URL")
                    .MaximumLength(500).WithMessage("URL must not exceed 500 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Twitter), () =>
            {
                RuleFor(x => x.Twitter)
                    .Must(BeAValidUrl).WithMessage("Twitter URL must be a valid URL")
                    .MaximumLength(500).WithMessage("URL must not exceed 500 characters");
            });

            When(x => !string.IsNullOrEmpty(x.LinkedIn), () =>
            {
                RuleFor(x => x.LinkedIn)
                    .Must(BeAValidUrl).WithMessage("LinkedIn URL must be a valid URL")
                    .MaximumLength(500).WithMessage("URL must not exceed 500 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Instagram), () =>
            {
                RuleFor(x => x.Instagram)
                    .Must(BeAValidUrl).WithMessage("Instagram URL must be a valid URL")
                    .MaximumLength(500).WithMessage("URL must not exceed 500 characters");
            });

            When(x => !string.IsNullOrEmpty(x.YouTube), () =>
            {
                RuleFor(x => x.YouTube)
                    .Must(BeAValidUrl).WithMessage("YouTube URL must be a valid URL")
                    .MaximumLength(500).WithMessage("URL must not exceed 500 characters");
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