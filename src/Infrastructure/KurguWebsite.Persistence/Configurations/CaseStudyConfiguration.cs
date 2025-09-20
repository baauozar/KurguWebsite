using KurguWebsite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Configurations
{
    public class CaseStudyConfiguration : AuditableEntityConfiguration<CaseStudy>
    {
        public override void Configure(EntityTypeBuilder<CaseStudy> builder)
        {
            base.Configure(builder);

            builder.ToTable("CaseStudies");

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(e => e.Slug)
                .IsUnique();

            builder.Property(e => e.ClientName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(e => e.Industry)
                .HasMaxLength(100);

            builder.Property(e => e.Challenge)
                .HasMaxLength(1000);

            builder.Property(e => e.Solution)
                .HasMaxLength(1000);

            builder.Property(e => e.Result)
                .HasMaxLength(1000);

            builder.Property(e => e.ImagePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.ThumbnailPath)
                .HasMaxLength(500);

            // Store Technologies as JSON
            builder.Property(e => e.Technologies)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
                .HasColumnType("nvarchar(max)");

            // Relationship
            builder.HasOne(e => e.Service)
                .WithMany(s => s.CaseStudies)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.IsFeatured);
            builder.HasIndex(e => e.CompletedDate);
            builder.HasIndex(e => e.ServiceId);
        }
    }
}