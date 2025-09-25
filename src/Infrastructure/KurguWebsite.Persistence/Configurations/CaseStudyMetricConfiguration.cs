using KurguWebsite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KurguWebsite.Persistence.Configurations
{
    public class CaseStudyMetricConfiguration : IEntityTypeConfiguration<CaseStudyMetric>
    {
        public void Configure(EntityTypeBuilder<CaseStudyMetric> builder)
        {
            builder.HasKey(csm => csm.Id);

            builder.Property(csm => csm.MetricName)
                .HasMaxLength(100);

            builder.Property(csm => csm.MetricValue)
                .HasMaxLength(100);

            builder.Property(csm => csm.Icon)
                .HasMaxLength(100);

            // This configuration now correctly links CaseStudy.Metrics to CaseStudyMetric
            builder.HasOne(csm => csm.CaseStudy)
                .WithMany(cs => cs.Metrics)
                .HasForeignKey(csm => csm.CaseStudyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}