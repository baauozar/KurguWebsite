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

            // Property Configurations
            builder.Property(e => e.MetricName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(e => e.MetricValue)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(e => e.Icon)
                .HasMaxLength(150);

            builder.Property(e => e.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Indexes
            builder.HasIndex(e => new { e.CaseStudyId, e.MetricName })
                .HasDatabaseName("IX_CaseStudyMetrics_CaseStudyId_MetricName");

            builder.HasIndex(e => new { e.CaseStudyId, e.DisplayOrder })
                .HasDatabaseName("IX_CaseStudyMetrics_CaseStudyId_DisplayOrder");

            // Relationships
            builder.HasOne(e => e.CaseStudy)
                .WithMany(cs => cs.Metrics)
                .HasForeignKey(e => e.CaseStudyId)
                .OnDelete(DeleteBehavior.Cascade); // Changed to Cascade - when case study is deleted, metrics should be deleted too

            // Soft-delete filter
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
