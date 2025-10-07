using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KurguWebsite.Persistence.Configurations
{
    // ✅ CORRECTED: Inherit from AuditableEntityConfiguration for consistency
    public class CompanyInfoConfiguration : AuditableEntityConfiguration<CompanyInfo>
    {
        public override void Configure(EntityTypeBuilder<CompanyInfo> builder)
        {
            // ✅ ADDED: Call the base configuration
            base.Configure(builder);

            builder.ToTable("CompanyInfo");

            // ===========================================
            // SCALAR PROPERTIES
            // ===========================================
            builder.Property(c => c.CompanyName).IsRequired().HasMaxLength(200);
            builder.Property(c => c.LogoPath).HasMaxLength(500);
            builder.Property(c => c.LogoLightPath).HasMaxLength(500);
            builder.Property(c => c.About).HasMaxLength(2000);
            builder.Property(c => c.Mission).HasMaxLength(1000);
            builder.Property(c => c.Vision).HasMaxLength(1000);
            builder.Property(c => c.Slogan).HasMaxLength(200);
            builder.Property(c => c.CopyrightText).HasMaxLength(200);

            // ✅ ADDED: Configuration for new properties
            builder.Property(c => c.YearsInBusiness).IsRequired().HasDefaultValue(0);
            builder.Property(c => c.TotalClients).IsRequired().HasDefaultValue(0);
            builder.Property(c => c.ProjectsCompleted).IsRequired().HasDefaultValue(0);
            builder.Property(c => c.TeamMembers).IsRequired().HasDefaultValue(0);
            builder.Property(c => c.MissionImagePath).HasMaxLength(500);
            builder.Property(c => c.VisionImagePath).HasMaxLength(500);
            builder.Property(c => c.CareersImagePath).HasMaxLength(500);

            // ===========================================
            // OWNED ENTITIES (VALUE OBJECTS)
            // ===========================================

            builder.OwnsOne(c => c.ContactInformation, contact =>
            {
                contact.Property(ci => ci.SupportPhone).HasMaxLength(20).IsRequired();
                contact.Property(ci => ci.SalesPhone).HasMaxLength(20).IsRequired();
                contact.Property(ci => ci.Email).HasMaxLength(100).IsRequired();
                contact.Property(ci => ci.SupportEmail).HasMaxLength(100);
                contact.Property(ci => ci.SalesEmail).HasMaxLength(100);
            });

            builder.OwnsOne(c => c.OfficeAddress, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200).IsRequired();
                address.Property(a => a.Suite).HasMaxLength(50);
                address.Property(a => a.City).HasMaxLength(100).IsRequired();
                address.Property(a => a.State).HasMaxLength(100).IsRequired();
                address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
                address.Property(a => a.Country).HasMaxLength(100).IsRequired();
            });

            builder.OwnsOne(c => c.SocialMedia, social =>
            {
                social.Property(s => s.Facebook).HasMaxLength(200);
                social.Property(s => s.Twitter).HasMaxLength(200);
                social.Property(s => s.LinkedIn).HasMaxLength(200);
                social.Property(s => s.Instagram).HasMaxLength(200);
                social.Property(s => s.YouTube).HasMaxLength(200);
            });
        }
    }
}