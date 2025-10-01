// Persistence/Configurations/CaseStudyMetricConfiguration.cs
using KurguWebsite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KurguWebsite.Persistence.Configurations
{
    public class CaseStudyMetricConfiguration : AuditableEntityConfiguration<CaseStudyMetric>
    {
        public override void Configure(EntityTypeBuilder<CaseStudyMetric> builder)
        {
            base.Configure(builder);

            builder.ToTable("CaseStudyMetrics");

            builder.Property(e => e.MetricName).HasMaxLength(150);
            builder.Property(e => e.MetricValue).HasMaxLength(150);
            builder.Property(e => e.Icon).HasMaxLength(150);

            builder.HasIndex(e => new { e.CaseStudyId, e.MetricName });

            builder.HasOne(e => e.CaseStudy)
                   .WithMany(cs => cs.Metrics)      // ensure CaseStudy has ICollection<CaseStudyMetric> Metrics
                   .HasForeignKey(e => e.CaseStudyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Soft-delete filter (uses AuditableEntity.IsDeleted)
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
