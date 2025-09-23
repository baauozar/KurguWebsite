using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{

    public class Testimonial : AuditableEntity
    {
        public string ClientName { get; private set; } = string.Empty;
        public string ClientTitle { get; private set; } = string.Empty;
        public string CompanyName { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public string? ClientImagePath { get; private set; }
        public int Rating { get; private set; }
        public DateTime TestimonialDate { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFeatured { get; private set; }
        public int DisplayOrder { get; private set; }


        private Testimonial() { }

        public static Testimonial Create(
            string clientName,
            string clientTitle,
            string companyName,
            string content,
            int rating = 5)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            var testimonial =  new Testimonial
            {
                ClientName = clientName,
                ClientTitle = clientTitle,
                CompanyName = companyName,
                Content = content,
                Rating = rating,
                TestimonialDate = DateTime.UtcNow,
                IsActive = true
            };
            testimonial.AddDomainEvent(new TestimonialCreatedEvent(testimonial.Id));
            return testimonial;
        }

        public void Update(
            string clientName,
            string clientTitle,
            string companyName,
            string content,
            int rating)
        {
            ClientName = clientName;
            ClientTitle = clientTitle;
            CompanyName = companyName;
            Content = content;
            Rating = rating;
            AddDomainEvent(new TestimonialUpdatedEvent(this.Id));
        }

        public void SetFeatured(bool isFeatured) => IsFeatured = isFeatured;
        public void SetDisplayOrder(int order) => DisplayOrder = order;
        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}


