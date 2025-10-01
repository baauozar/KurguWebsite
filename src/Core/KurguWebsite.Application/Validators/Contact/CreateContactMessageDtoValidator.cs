using FluentValidation;
using KurguWebsite.Application.Features.ContactMessages.Commands;

namespace KurguWebsite.Application.Validators.Contact
{
    public class CreateContactMessageDtoValidator : AbstractValidator<SubmitContactMessageCommand>
    {
        public CreateContactMessageDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Name can only contain letters and spaces");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
                .WithMessage("Invalid email format");
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Invalid phone number format")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Subject is required")
                .MaximumLength(200).WithMessage("Subject must not exceed 200 characters");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required")
                .MinimumLength(10).WithMessage("Message must be at least 10 characters")
                .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters");
        }
    }
}