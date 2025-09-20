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
    public class ServiceConfiguration : AuditableEntityConfiguration<Service>
    {
        public override void Configure(EntityTypeBuilder<Service> builder)
        {
            base.Configure(builder);

            builder.ToTable("Services");

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(e => e.Slug)
                .IsUnique();

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(e => e.ShortDescription)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(e => e.FullDescription)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.IconPath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.IconClass)
                .HasMaxLength(100);

            builder.Property(e => e.Category)
                .IsRequired();

            builder.Property(e => e.MetaTitle)
                .HasMaxLength(60);

            builder.Property(e => e.MetaDescription)
                .HasMaxLength(160);

            builder.Property(e => e.MetaKeywords)
                .HasMaxLength(500);

            // Relationships
            builder.HasMany(e => e.Features)
                .WithOne(f => f.Service)
                .HasForeignKey(f => f.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.CaseStudies)
                .WithOne(c => c.Service)
                .HasForeignKey(c => c.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.IsFeatured);
            builder.HasIndex(e => e.Category);
            builder.HasIndex(e => e.DisplayOrder);
        }
    }
}