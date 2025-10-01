// File: src/Persistence/KurguWebsite.Persistence/Configurations/CaseStudyConfiguration.cs
using KurguWebsite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking; // ValueComparer
using System.Collections.Generic;
using System.Linq; // SequenceEqual, Aggregate
using System.Text.Json;

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
                .HasMaxLength(256);

            builder.HasIndex(e => e.Slug).IsUnique();

            builder.Property(e => e.ClientName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(e => e.Industry).HasMaxLength(100);
            builder.Property(e => e.Challenge).HasMaxLength(1000);
            builder.Property(e => e.Solution).HasMaxLength(1000);
            builder.Property(e => e.Result).HasMaxLength(1000);

            builder.Property(e => e.ImagePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.ThumbnailPath).HasMaxLength(500);

            // ---- Technologies (JSON) mapped to backing field _technologies as List<string> ----
            var techField = builder.Property<List<string>>("_technologies");

            // JSON conversion
            techField.HasConversion(
                v => JsonSerializer.Serialize(v ?? new List<string>(), (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            );

            // Comparer for List<string> (no null-propagation)
            var listComparer = new ValueComparer<List<string>>(
                (a, b) =>
                    (a == null && b == null) ||
                    (a != null && b != null && a.SequenceEqual(b)),
                v => v == null
                    ? 0
                    : v.Aggregate(0, (hash, s) => System.HashCode.Combine(hash, (s == null ? 0 : s.GetHashCode()))),
                v => v == null ? null : new List<string>(v) // snapshot
            );

            // Apply comparer and column type; use field access mode
            techField.Metadata.SetValueComparer(listComparer);
            techField.HasColumnType("nvarchar(max)");
            techField.UsePropertyAccessMode(PropertyAccessMode.Field);

            // ---- Relationship ----
            builder.HasOne(e => e.Service)
                   .WithMany(s => s.CaseStudies)
                   .HasForeignKey(e => e.ServiceId)
                   .OnDelete(DeleteBehavior.SetNull); // ensure CaseStudy.ServiceId is Guid?

            // ---- Indexes ----
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.IsFeatured);
            builder.HasIndex(e => e.CompletedDate);
            builder.HasIndex(e => e.ServiceId);
        }
    }
}
