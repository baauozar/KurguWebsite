using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class CompanyInfo : BaseEntity, IAggregateRoot
    {
        public string CompanyName { get; private set; } = string.Empty;
        public string? LogoPath { get; private set; }
        public string? LogoLightPath { get; private set; }
        public string? About { get; private set; }
        public string? Mission { get; private set; }
        public string? Vision { get; private set; }
        public string? Slogan { get; private set; }
        public string? CopyrightText { get; private set; }

        // Value Objects
        public ContactInfo ContactInformation { get; private set; } = null!;
        public Address OfficeAddress { get; private set; } = null!;
        public SocialMediaLinks? SocialMedia { get; private set; }

        private CompanyInfo() { }

        public static CompanyInfo Create(
            string companyName,
            ContactInfo contactInfo,
            Address address)
        {
            return new CompanyInfo
            {
                CompanyName = companyName,
                ContactInformation = contactInfo,
                OfficeAddress = address,
                CopyrightText = $"{companyName}. © {DateTime.Now.Year}. All Rights Reserved"
            };
        }

        public void UpdateBasicInfo(
            string companyName,
            string? about,
            string? mission,
            string? vision,
            string? slogan)
        {
            CompanyName = companyName;
            About = about;
            Mission = mission;
            Vision = vision;
            Slogan = slogan;
            CopyrightText = $"{companyName}. © {DateTime.Now.Year}. All Rights Reserved";
        }

        public void UpdateLogos(string? logoPath, string? logoLightPath)
        {
            LogoPath = logoPath;
            LogoLightPath = logoLightPath;
        }

        public void UpdateContactInfo(ContactInfo contactInfo)
        {
            ContactInformation = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        }

        public void UpdateAddress(Address address)
        {
            OfficeAddress = address ?? throw new ArgumentNullException(nameof(address));
        }

        public void UpdateSocialMedia(SocialMediaLinks? socialMedia)
        {
            SocialMedia = socialMedia;
        }
    }
}