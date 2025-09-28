// File: src/Persistence/KurguWebsite.Persistence/Configurations/ServiceFeatureConfiguration.cs
using KurguWebsite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.Property(e => e.ServiceId)
                .IsRequired();

            // Relationship
            builder.HasOne(e => e.Service)
                .WithMany(s => s.Features)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => new { e.ServiceId, e.DisplayOrder }).IsUnique(); // per-service ordering
            // Optional: prevent duplicate titles within a service
            // builder.HasIndex(e => new { e.ServiceId, e.Title }).IsUnique();
        }
    }
}
