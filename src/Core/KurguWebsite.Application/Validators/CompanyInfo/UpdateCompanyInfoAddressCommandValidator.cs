using FluentValidation;
using KurguWebsite.Application.Features.CompanyInfo.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.CompanyInfo
{
    public class UpdateCompanyInfoAddressCommandValidator : AbstractValidator<UpdateCompanyInfoAddressCommand>
    {
        public UpdateCompanyInfoAddressCommandValidator()
        {
            When(x => !string.IsNullOrEmpty(x.Street), () =>
            {
                RuleFor(x => x.Street)
                    .MaximumLength(200).WithMessage("Street must not exceed 200 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Suite), () =>
            {
                RuleFor(x => x.Suite)
                    .MaximumLength(50).WithMessage("Suite must not exceed 50 characters");
            });

            When(x => !string.IsNullOrEmpty(x.City), () =>
            {
                RuleFor(x => x.City)
                    .MaximumLength(100).WithMessage("City must not exceed 100 characters");
            });

            When(x => !string.IsNullOrEmpty(x.State), () =>
            {
                RuleFor(x => x.State)
                    .MaximumLength(100).WithMessage("State must not exceed 100 characters");
            });

            When(x => !string.IsNullOrEmpty(x.PostalCode), () =>
            {
                RuleFor(x => x.PostalCode)
                    .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Country), () =>
            {
                RuleFor(x => x.Country)
                    .MaximumLength(100).WithMessage("Country must not exceed 100 characters");
            });
        }
    }
}