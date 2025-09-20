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
    public class ProcessStepConfiguration : AuditableEntityConfiguration<ProcessStep>
    {
        public override void Configure(EntityTypeBuilder<ProcessStep> builder)
        {
            base.Configure(builder);

            builder.ToTable("ProcessSteps");

            builder.Property(e => e.StepNumber)
                .IsRequired();

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

            // Indexes
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.DisplayOrder);
            builder.HasIndex(e => e.StepNumber);
        }
    }
}