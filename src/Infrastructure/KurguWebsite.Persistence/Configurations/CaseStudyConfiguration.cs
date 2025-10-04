// File: src/Persistence/KurguWebsite.Persistence/Configurations/CaseStudyConfiguration.cs
using KurguWebsite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace KurguWebsite.Persistence.Configurations
{
    public class CaseStudyConfiguration : AuditableEntityConfiguration<CaseStudy>
    {
        public override void Configure(EntityTypeBuilder<CaseStudy> builder)
        {
            base.Configure(builder);

            builder.ToTable("CaseStudies");

            // ===========================================
            // STRING PROPERTIES
            // ===========================================
            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(256);

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

            // ===========================================
            // TECHNOLOGIES (JSON) - FIXED
            // ===========================================
            builder.Property<List<string>>("_technologies")
                .HasColumnName("Technologies") // ✅ ADDED: Explicit column name
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    // To database
                    v => JsonSerializer.Serialize(v ?? new List<string>(), (JsonSerializerOptions?)null),
                    // From database
                    v => string.IsNullOrWhiteSpace(v)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
                    // ✅ ADDED: ValueComparer as third parameter (modern approach)
                    new ValueComparer<List<string>>(
                        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                        v => v == null ? 0 : v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s == null ? 0 : s.GetHashCode())),
                        v => v == null ? new List<string>() : new List<string>(v)
                    )
                )
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // ===========================================
            // NAVIGATION PROPERTIES
            // ===========================================

            // ✅ ADDED: Metrics relationship
            builder.HasMany(cs => cs.Metrics)
                .WithOne(m => m.CaseStudy)
                .HasForeignKey(m => m.CaseStudyId)
                .OnDelete(DeleteBehavior.Cascade); // Delete metrics when case study is deleted

            // Service relationship
            builder.HasOne(cs => cs.Service)
                .WithMany(s => s.CaseStudies)
                .HasForeignKey(cs => cs.ServiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // ===========================================
            // INDEXES
            // ===========================================
            builder.HasIndex(cs => cs.Slug)
                .IsUnique()
                .HasDatabaseName("IX_CaseStudies_Slug");

            builder.HasIndex(cs => cs.ClientName)
                .HasDatabaseName("IX_CaseStudies_ClientName");

            builder.HasIndex(cs => cs.IsActive)
                .HasDatabaseName("IX_CaseStudies_IsActive");

            builder.HasIndex(cs => cs.IsFeatured)
                .HasDatabaseName("IX_CaseStudies_IsFeatured");

            builder.HasIndex(cs => cs.CompletedDate)
                .HasDatabaseName("IX_CaseStudies_CompletedDate");

            builder.HasIndex(cs => cs.ServiceId)
                .HasDatabaseName("IX_CaseStudies_ServiceId");

            // Composite index for featured active case studies
            builder.HasIndex(cs => new { cs.IsFeatured, cs.IsActive, cs.DisplayOrder })
                .HasDatabaseName("IX_CaseStudies_Featured_Active_DisplayOrder");

            // ✅ ADDED: Explicit soft delete filter
            builder.HasQueryFilter(cs => !cs.IsDeleted);
        }
    }
}