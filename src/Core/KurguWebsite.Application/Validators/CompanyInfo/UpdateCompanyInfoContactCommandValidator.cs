using FluentValidation;
using KurguWebsite.Application.Features.CompanyInfo.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.CompanyInfo
{
    public class UpdateCompanyInfoContactCommandValidator : AbstractValidator<UpdateCompanyInfoContactCommand>
    {
        public UpdateCompanyInfoContactCommandValidator()
        {
            When(x => !string.IsNullOrEmpty(x.SupportPhone), () =>
            {
                RuleFor(x => x.SupportPhone)
                    .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Invalid phone number format")
                    .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");
            });

            When(x => !string.IsNullOrEmpty(x.SalesPhone), () =>
            {
                RuleFor(x => x.SalesPhone)
                    .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Invalid phone number format")
                    .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Email), () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress().WithMessage("Invalid email format")
                    .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
            });

            When(x => !string.IsNullOrEmpty(x.SupportEmail), () =>
            {
                RuleFor(x => x.SupportEmail)
                    .EmailAddress().WithMessage("Invalid support email format")
                    .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
            });

            When(x => !string.IsNullOrEmpty(x.SalesEmail), () =>
            {
                RuleFor(x => x.SalesEmail)
                    .EmailAddress().WithMessage("Invalid sales email format")
                    .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
            });
        }
    }
}