// src/Core/KurguWebsite.Domain/Specifications/TestimonialSpecifications.cs
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Domain.Specifications
{
    /// <summary>
    /// Get active testimonials
    /// </summary>
    public class ActiveTestimonialsSpecification : BaseSpecification<Testimonial>
    {
        public ActiveTestimonialsSpecification()
            : base(t => t.IsActive && !t.IsDeleted)
        {
            ApplyOrderBy(t => t.DisplayOrder);
        }
    }

    /// <summary>
    /// Get featured testimonials
    /// </summary>
    public class FeaturedTestimonialsSpecification : BaseSpecification<Testimonial>
    {
        public FeaturedTestimonialsSpecification()
            : base(t => t.IsFeatured && t.IsActive && !t.IsDeleted)
        {
            ApplyOrderBy(t => t.DisplayOrder);
        }
    }

    /// <summary>
    /// Get testimonials with high rating
    /// </summary>
    public class HighRatingTestimonialsSpecification : BaseSpecification<Testimonial>
    {
        public HighRatingTestimonialsSpecification(int minRating = 4)
            : base(t => t.Rating >= minRating && t.IsActive && !t.IsDeleted)
        {
            ApplyOrderByDescending(t => t.Rating);
            ApplyOrderBy(t => t.DisplayOrder);
        }
    }
}