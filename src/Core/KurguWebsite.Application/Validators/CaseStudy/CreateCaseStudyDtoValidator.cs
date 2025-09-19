using FluentValidation;
using KurguWebsite.Application.DTOs.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.CaseStudy
{
    public class CreateCaseStudyDtoValidator : AbstractValidator<CreateCaseStudyDto>
    {
        public CreateCaseStudyDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

            RuleFor(x => x.ClientName)
                .NotEmpty().WithMessage("Client name is required")
                .MaximumLength(100).WithMessage("Client name must not exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

            RuleFor(x => x.ImagePath)
                .NotEmpty().WithMessage("Image is required");

            RuleFor(x => x.CompletedDate)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Completion date cannot be in the future");

            When(x => !string.IsNullOrEmpty(x.Challenge), () =>
            {
                RuleFor(x => x.Challenge)
                    .MaximumLength(1000).WithMessage("Challenge must not exceed 1000 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Solution), () =>
            {
                RuleFor(x => x.Solution)
                    .MaximumLength(1000).WithMessage("Solution must not exceed 1000 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Result), () =>
            {
                RuleFor(x => x.Result)
                    .MaximumLength(1000).WithMessage("Result must not exceed 1000 characters");
            });
        }
    }
}