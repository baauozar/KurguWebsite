using KurguWebsite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KurguWebsite.Persistence.Configurations
{
    public class AuditLogConfiguration : AuditableEntityConfiguration<AuditLog>
    {
        public override void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            base.Configure(builder);

            builder.Property(a => a.UserId).HasMaxLength(450);
            builder.Property(a => a.UserName).HasMaxLength(256);
            builder.Property(a => a.Action).HasMaxLength(50);
            builder.Property(a => a.EntityType).HasMaxLength(256);
            builder.Property(a => a.EntityId).HasMaxLength(450);
            builder.Property(a => a.IpAddress).HasMaxLength(50);

            builder.Property(a => a.OldValues).HasColumnType("nvarchar(max)");
            builder.Property(a => a.NewValues).HasColumnType("nvarchar(max)");
        }
    }
}
