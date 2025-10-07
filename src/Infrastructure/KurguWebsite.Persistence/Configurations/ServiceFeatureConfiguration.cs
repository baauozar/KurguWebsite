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

            // Property Configurations
            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.IconClass)
                .HasMaxLength(100);

            // ✅ Make sure DisplayOrder is required
            builder.Property(e => e.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.ServiceId)
                .IsRequired();

            // Relationship
            builder.HasOne(e => e.Service)
                .WithMany(s => s.Features)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            // ✅ CORRECTED: Removed duplicate index and the IsUnique() constraint.
            // This prevents potential conflicts when reordering soft-deleted items.
            builder.HasIndex(e => new { e.ServiceId, e.DisplayOrder });

            // Global soft-delete filter
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}