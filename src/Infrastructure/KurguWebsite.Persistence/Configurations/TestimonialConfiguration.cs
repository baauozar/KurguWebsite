using KurguWebsite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Configurations
{
    public class TestimonialConfiguration : AuditableEntityConfiguration<Testimonial>
    {
        public override void Configure(EntityTypeBuilder<Testimonial> builder)
        {
            base.Configure(builder);

            builder.ToTable("Testimonials");

            builder.Property(e => e.ClientName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.ClientTitle)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(e => e.ClientImagePath)
                .HasMaxLength(500);

            builder.Property(e => e.Rating)
                .IsRequired()
                .HasDefaultValue(5);

            // Indexes
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.IsFeatured);
            builder.HasIndex(e => e.TestimonialDate);
        }
    }
}