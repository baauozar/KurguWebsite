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
                .HasMaxLength(200);

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

            // ---- Technologies (JSON) with proper change tracking ----
            var techComparer = new ValueComparer<List<string>>(
       // Equals
       (a, b) =>
           (a == null && b == null) ||
           (a != null && b != null && a.SequenceEqual(b)),

       // Hash code
       v => v == null
           ? 0
           : v.Aggregate(0, (hash, s) => HashCode.Combine(hash, (s == null ? 0 : s.GetHashCode()))),

       // Snapshot (deep copy)
       v => v == null ? null : new List<string>(v)
   );

            var techProp = builder.Property(e => e.Technologies);

            techProp.HasConversion(
                v => JsonSerializer.Serialize(v ?? new List<string>(), (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            );

            // Set comparer on the property metadata (cannot be chained)
            techProp.Metadata.SetValueComparer(techComparer);

            // Then set the column type (must be a separate statement)
            techProp.HasColumnType("nvarchar(max)");

            // ---- Relationship ----
            builder.HasOne(e => e.Service)
                   .WithMany(s => s.CaseStudies)
                   .HasForeignKey(e => e.ServiceId)
                   .OnDelete(DeleteBehavior.SetNull); // ensure ServiceId is Guid?

            // ---- Indexes ----
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.IsFeatured);
            builder.HasIndex(e => e.CompletedDate);
            builder.HasIndex(e => e.ServiceId);
        }
    }
}
