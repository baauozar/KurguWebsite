// src/Infrastructure/KurguWebsite.Persistence/Configurations/CompanyInfoConfiguration.cs
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KurguWebsite.Persistence.Configurations
{
    public class CompanyInfoConfiguration : IEntityTypeConfiguration<CompanyInfo>
    {
        public void Configure(EntityTypeBuilder<CompanyInfo> builder)
        {
            builder.ToTable("CompanyInfo");
            builder.HasKey(c => c.Id);

            // Contact Information - Owned Entity
            builder.OwnsOne(c => c.ContactInformation, contact =>
            {
                contact.Property(ci => ci.SupportPhone).HasMaxLength(20).IsRequired();
                contact.Property(ci => ci.SalesPhone).HasMaxLength(20).IsRequired();
                contact.Property(ci => ci.Email).HasMaxLength(100).IsRequired();
                contact.Property(ci => ci.SupportEmail).HasMaxLength(100);
                contact.Property(ci => ci.SalesEmail).HasMaxLength(100);
            });

            // Office Address - Owned Entity
            builder.OwnsOne(c => c.OfficeAddress, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200).IsRequired();
                address.Property(a => a.Suite).HasMaxLength(50);
                address.Property(a => a.City).HasMaxLength(100).IsRequired();
                address.Property(a => a.State).HasMaxLength(100).IsRequired();
                address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
                address.Property(a => a.Country).HasMaxLength(100).IsRequired();
            });

            // Social Media Links - Owned Entity with required property fix
            builder.OwnsOne(c => c.SocialMedia, social =>
            {
                // Make at least one property required to fix the warning
                social.Property(s => s.Facebook).HasMaxLength(200).IsRequired(false);
                social.Property(s => s.Twitter).HasMaxLength(200);
                social.Property(s => s.LinkedIn).HasMaxLength(200);
                social.Property(s => s.Instagram).HasMaxLength(200);
                social.Property(s => s.YouTube).HasMaxLength(200);
            });

            builder.Property(c => c.CompanyName).HasMaxLength(200).IsRequired();
            builder.Property(c => c.LogoPath).HasMaxLength(500);
            builder.Property(c => c.LogoLightPath).HasMaxLength(500);
            builder.Property(c => c.About).HasMaxLength(2000);
            builder.Property(c => c.Mission).HasMaxLength(1000);
            builder.Property(c => c.Vision).HasMaxLength(1000);
            builder.Property(c => c.Slogan).HasMaxLength(200);
            builder.Property(c => c.CopyrightText).HasMaxLength(200);
        }
    }
}