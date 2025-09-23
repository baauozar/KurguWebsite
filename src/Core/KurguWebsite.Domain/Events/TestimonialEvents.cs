using KurguWebsite.Domain.Entities;
using System;

namespace KurguWebsite.Domain.Events
{
    public class TestimonialCreatedEvent : GenericDomainEvent<Testimonial>
    {
        public TestimonialCreatedEvent(Guid testimonialId) : base(testimonialId) { }
    }

    public class TestimonialUpdatedEvent : GenericDomainEvent<Testimonial>
    {
        public TestimonialUpdatedEvent(Guid testimonialId) : base(testimonialId) { }
    }

    public class TestimonialDeletedEvent : GenericDomainEvent<Testimonial>
    {
        public TestimonialDeletedEvent(Guid testimonialId) : base(testimonialId) { }
    }
}