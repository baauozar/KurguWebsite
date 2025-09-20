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
    public class CompanyInfoConfiguration : AuditableEntityConfiguration<CompanyInfo>
    {
        public override void Configure(EntityTypeBuilder<CompanyInfo> builder)
        {
            base.Configure(builder);

            builder.ToTable("CompanyInfo");

            builder.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LogoPath)
                .HasMaxLength(500);

            builder.Property(e => e.LogoLightPath)
                .HasMaxLength(500);

            builder.Property(e => e.About)
                .HasMaxLength(2000);

            builder.Property(e => e.Mission)
                .HasMaxLength(1000);

            builder.Property(e => e.Vision)
                .HasMaxLength(1000);

            builder.Property(e => e.Slogan)
                .HasMaxLength(200);

            builder.Property(e => e.CopyrightText)
                .HasMaxLength(200);

            // ContactInfo value object
            builder.OwnsOne(e => e.ContactInformation, contact =>
            {
                contact.Property(c => c.SupportPhone)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("SupportPhone");

                contact.Property(c => c.SalesPhone)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("SalesPhone");

                contact.Property(c => c.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("Email");

                contact.Property(c => c.SupportEmail)
                    .HasMaxLength(100)
                    .HasColumnName("SupportEmail");

                contact.Property(c => c.SalesEmail)
                    .HasMaxLength(100)
                    .HasColumnName("SalesEmail");
            });

            // Address value object
            builder.OwnsOne(e => e.OfficeAddress, address =>
            {
                address.Property(a => a.Street)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("Street");

                address.Property(a => a.Suite)
                    .HasMaxLength(50)
                    .HasColumnName("Suite");

                address.Property(a => a.City)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("City");

                address.Property(a => a.State)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("State");

                address.Property(a => a.PostalCode)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("PostalCode");

                address.Property(a => a.Country)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("Country");
            });

            // SocialMediaLinks value object
            builder.OwnsOne(e => e.SocialMedia, social =>
            {
                social.Property(s => s.Facebook)
                    .HasMaxLength(200)
                    .HasColumnName("Facebook");

                social.Property(s => s.Twitter)
                    .HasMaxLength(200)
                    .HasColumnName("Twitter");

                social.Property(s => s.LinkedIn)
                    .HasMaxLength(200)
                    .HasColumnName("LinkedIn");

                social.Property(s => s.Instagram)
                    .HasMaxLength(200)
                    .HasColumnName("Instagram");

                social.Property(s => s.YouTube)
                    .HasMaxLength(200)
                    .HasColumnName("YouTube");
            });
        }
    }
}