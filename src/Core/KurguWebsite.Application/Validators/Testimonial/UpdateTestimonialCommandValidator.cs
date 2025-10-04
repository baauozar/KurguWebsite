using FluentValidation;
using KurguWebsite.Application.Features.Testimonials.Commands;

namespace KurguWebsite.Application.Validators.Testimonial
{
    public class UpdateTestimonialCommandValidator : AbstractValidator<UpdateTestimonialCommand>
    {
        public UpdateTestimonialCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");

            When(x => x.ClientName != null, () =>
            {
                RuleFor(x => x.ClientName)
                    .NotEmpty().WithMessage("Client name cannot be empty")
                    .MaximumLength(100).WithMessage("Client name must not exceed 100 characters");
            });

            When(x => x.ClientTitle != null, () =>
            {
                RuleFor(x => x.ClientTitle)
                    .NotEmpty().WithMessage("Client title cannot be empty")
                    .MaximumLength(100).WithMessage("Client title must not exceed 100 characters");
            });

            When(x => x.CompanyName != null, () =>
            {
                RuleFor(x => x.CompanyName)
                    .NotEmpty().WithMessage("Company name cannot be empty")
                    .MaximumLength(100).WithMessage("Company name must not exceed 100 characters");
            });

            When(x => x.Content != null, () =>
            {
                RuleFor(x => x.Content)
                    .NotEmpty().WithMessage("Content cannot be empty")
                    .MinimumLength(20).WithMessage("Testimonial must be at least 20 characters")
                    .MaximumLength(1000).WithMessage("Testimonial must not exceed 1000 characters");
            });

            When(x => x.Rating.HasValue, () =>
            {
                RuleFor(x => x.Rating)
                    .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
            });

            When(x => !string.IsNullOrEmpty(x.ClientImagePath), () =>
            {
                RuleFor(x => x.ClientImagePath)
                    .MaximumLength(500).WithMessage("Image path must not exceed 500 characters");
            });
        }
    }
}