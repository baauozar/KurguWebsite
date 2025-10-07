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
    public class PartnerConfiguration : AuditableEntityConfiguration<Partner>
    {
        public override void Configure(EntityTypeBuilder<Partner> builder)
        {
            base.Configure(builder);

            builder.ToTable("Partners");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LogoPath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.WebsiteUrl)
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.Type)
                .IsRequired();
            builder.Property(e => e.DisplayOrder)
               .IsRequired()
               .HasDefaultValue(0);
            // Indexes
            builder.HasIndex(e => new { e.Type, e.IsActive, e.DisplayOrder });
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.Type);
            builder.HasIndex(e => e.DisplayOrder);
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}