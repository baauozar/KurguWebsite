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
    public class RefreshTokenConfiguration : BaseEntityConfiguration<RefreshToken>
    {
        public override void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            base.Configure(builder);

            builder.ToTable("RefreshTokens");

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(e => e.Token)
                .IsUnique();

            builder.Property(e => e.ReplacedByToken)
                .HasMaxLength(500);

            builder.Property(e => e.Expires)
                .IsRequired();

            builder.Property(e => e.CreatedByIp)
                .IsRequired()
                .HasMaxLength(45);

            builder.Property(e => e.RevokedByIp)
                .HasMaxLength(45);

            // Indexes
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.Expires);
            builder.HasIndex(e => new { e.Token, e.UserId, e.IsRevoked });
        }
    }
}