using KurguWebsite.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Configurations
{
    public abstract class AuditableEntityConfiguration<T> : BaseEntityConfiguration<T> where T : AuditableEntity
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(e => e.DeletedBy)
                .HasMaxLength(100);

            // Global query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
