using FluentValidation;
using KurguWebsite.Application.Features.CompanyInfo.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.CompanyInfo
{
    public class UpdateCompanyInfoBasicCommandValidator : AbstractValidator<UpdateCompanyInfoBasicCommand>
    {
        public UpdateCompanyInfoBasicCommandValidator()
        {
            When(x => !string.IsNullOrEmpty(x.CompanyName), () =>
            {
                RuleFor(x => x.CompanyName)
                    .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");
            });

            When(x => !string.IsNullOrEmpty(x.About), () =>
            {
                RuleFor(x => x.About)
                    .MaximumLength(2000).WithMessage("About must not exceed 2000 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Mission), () =>
            {
                RuleFor(x => x.Mission)
                    .MaximumLength(1000).WithMessage("Mission must not exceed 1000 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Vision), () =>
            {
                RuleFor(x => x.Vision)
                    .MaximumLength(1000).WithMessage("Vision must not exceed 1000 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Slogan), () =>
            {
                RuleFor(x => x.Slogan)
                    .MaximumLength(200).WithMessage("Slogan must not exceed 200 characters");
            });

            When(x => !string.IsNullOrEmpty(x.LogoPath), () =>
            {
                RuleFor(x => x.LogoPath)
                    .MaximumLength(500).WithMessage("Logo path must not exceed 500 characters");
            });

            When(x => !string.IsNullOrEmpty(x.LogoLightPath), () =>
            {
                RuleFor(x => x.LogoLightPath)
                    .MaximumLength(500).WithMessage("Logo light path must not exceed 500 characters");
            });
        }
    }
}