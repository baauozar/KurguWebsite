using FluentValidation;
using KurguWebsite.Application.Features.ContactMessages.Commands;

namespace KurguWebsite.Application.Validators.Contact
{
    public class ReplyToContactMessageCommandValidator : AbstractValidator<MarkContactMessageAsRepliedCommand>
    {
        public ReplyToContactMessageCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Contact message ID is required");

            RuleFor(x => x.ReplyMessage)
                .NotEmpty().WithMessage("Reply message is required")
                .MinimumLength(10).WithMessage("Reply message must be at least 10 characters")
                .MaximumLength(5000).WithMessage("Reply message must not exceed 5000 characters");
        }
    }
}