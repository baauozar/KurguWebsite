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
    public class ServiceFeatureConfiguration : AuditableEntityConfiguration<ServiceFeature>
    {
        public override void Configure(EntityTypeBuilder<ServiceFeature> builder)
        {
            base.Configure(builder);

            builder.ToTable("ServiceFeatures");

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.IconClass)
                .HasMaxLength(100);

            builder.Property(e => e.DisplayOrder)
                .HasDefaultValue(0);

            // Relationship
            builder.HasOne(e => e.Service)
                .WithMany(s => s.Features)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => e.ServiceId);
            builder.HasIndex(e => e.DisplayOrder);
        }
    }
}