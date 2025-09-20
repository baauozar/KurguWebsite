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
    public class ContactMessageConfiguration : BaseEntityConfiguration<ContactMessage>
    {
        public override void Configure(EntityTypeBuilder<ContactMessage> builder)
        {
            base.Configure(builder);

            builder.ToTable("ContactMessages");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(e => e.RepliedBy)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(e => e.IsRead);
            builder.HasIndex(e => e.IsReplied);
            builder.HasIndex(e => e.CreatedDate);
        }
    }
}