using FluentValidation;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Application.Features.Partners.Commands;

namespace KurguWebsite.Application.Validators.Partner
{
    public class CreatePartnerCommandValidator : AbstractValidator<CreatePartnerCommand>
    {
        public CreatePartnerCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Partner name is required")
                .MaximumLength(100).WithMessage("Partner name must not exceed 100 characters");

            RuleFor(x => x.LogoPath)
                .NotEmpty().WithMessage("Logo is required");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid partner type");

            RuleFor(x => x.WebsiteUrl)
      .Must(url =>
      {
          if (string.IsNullOrWhiteSpace(url)) return true;
          var candidate = url.Contains("://") ? url : $"https://{url}";
          return Uri.TryCreate(candidate, UriKind.Absolute, out var u)
                 && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps)
                 && !string.IsNullOrWhiteSpace(u.Host);
      })
      .WithMessage("Website must be a valid http(s) URL.");
        }


    }
}