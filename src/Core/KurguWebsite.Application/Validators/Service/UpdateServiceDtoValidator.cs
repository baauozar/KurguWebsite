using FluentValidation;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.Service
{
    public class UpdateServiceDtoValidator : AbstractValidator<UpdateServiceDto>

    {
        private readonly IServiceUniquenessChecker _checker;
        public UpdateServiceDtoValidator(IServiceUniquenessChecker checker)
        {
            _checker = checker;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
                .MustAsync(async (cmd, title, ct) =>
                    !await _checker.TitleExistsAsync(title, cmd.Id, ct))
                .WithMessage("A service with this title already exists.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Short description is required")
                .MaximumLength(300).WithMessage("Short description must not exceed 300 characters");

            RuleFor(x => x.IconPath)
                .NotEmpty().WithMessage("Icon is required");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid service category");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");

        }
    }
}
