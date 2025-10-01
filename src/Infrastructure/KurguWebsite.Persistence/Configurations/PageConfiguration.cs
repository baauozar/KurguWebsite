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
    public class PageConfiguration : AuditableEntityConfiguration<Page>
    {
        public override void Configure(EntityTypeBuilder<Page> builder)
        {
            base.Configure(builder);

            builder.ToTable("Pages");

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(265);

            builder.HasIndex(e => e.Slug)
                .IsUnique();

            builder.Property(e => e.Content)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.PageType)
                .IsRequired();

            builder.HasIndex(e => e.PageType)
                .IsUnique();

            // Hero Section
            builder.Property(e => e.HeroTitle)
                .HasMaxLength(200);

            builder.Property(e => e.HeroSubtitle)
                .HasMaxLength(200);

            builder.Property(e => e.HeroDescription)
                .HasMaxLength(500);

            builder.Property(e => e.HeroBackgroundImage)
                .HasMaxLength(500);

            builder.Property(e => e.HeroButtonText)
                .HasMaxLength(50);

            builder.Property(e => e.HeroButtonUrl)
                .HasMaxLength(200);

            // SEO
            builder.Property(e => e.MetaTitle)
                .HasMaxLength(60);

            builder.Property(e => e.MetaDescription)
                .HasMaxLength(160);

            builder.Property(e => e.MetaKeywords)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(e => e.IsActive);
        }
    }
}