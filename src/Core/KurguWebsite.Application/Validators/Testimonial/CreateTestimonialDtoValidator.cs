using FluentValidation;
using KurguWebsite.Application.DTOs.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Validators.Testimonial
{
    public class CreateTestimonialDtoValidator : AbstractValidator<CreateTestimonialDto>
    {
        public CreateTestimonialDtoValidator()
        {
            RuleFor(x => x.ClientName)
                .NotEmpty().WithMessage("Client name is required")
                .MaximumLength(100).WithMessage("Client name must not exceed 100 characters");

            RuleFor(x => x.ClientTitle)
                .NotEmpty().WithMessage("Client title is required")
                .MaximumLength(100).WithMessage("Client title must not exceed 100 characters");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(100).WithMessage("Company name must not exceed 100 characters");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Testimonial content is required")
                .MinimumLength(20).WithMessage("Testimonial must be at least 20 characters")
                .MaximumLength(1000).WithMessage("Testimonial must not exceed 1000 characters");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
        }
    }
}
