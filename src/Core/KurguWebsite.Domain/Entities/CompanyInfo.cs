using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class CompanyInfo : AuditableEntity, IAggregateRoot
    {
        public string CompanyName { get; private set; } = string.Empty;
        public string? LogoPath { get; private set; }
        public string? LogoLightPath { get; private set; }
        public string? About { get; private set; }
        public string? Mission { get; private set; }
        public string? Vision { get; private set; }
        public string? Slogan { get; private set; }
        public string? CopyrightText { get; private set; }
        public int YearsInBusiness { get; private set; }
        public int TotalClients { get; private set; }
        public int ProjectsCompleted { get; private set; }
        public int TeamMembers { get; private set; }

        // Mission/Vision images for the cards
        public string? MissionImagePath { get; private set; }
        public string? VisionImagePath { get; private set; }
        public string? CareersImagePath { get; private set; }

        // Value Objects
        public ContactInfo ContactInformation { get; private set; } = null!;
        public Address OfficeAddress { get; private set; } = null!;
        public SocialMediaLinks? SocialMedia { get; private set; }

        private CompanyInfo() { }

        public static CompanyInfo Create(
        string companyName,
        ContactInfo contactInfo,
        Address address,
        int yearsInBusiness = 0,
        int totalClients = 0,
        int projectsCompleted = 0,
        int teamMembers = 0)
        {
            return new CompanyInfo
            {
                CompanyName = companyName,
                ContactInformation = contactInfo,
                OfficeAddress = address,
                YearsInBusiness = yearsInBusiness,
                TotalClients = totalClients,
                ProjectsCompleted = projectsCompleted,
                TeamMembers = teamMembers,
                CopyrightText = $"{companyName}. © {DateTime.Now.Year}. All Rights Reserved"
            };
        }

        public void UpdateBasicInfo(
          string companyName,
          string? about,
          string? mission,
          string? vision,
          string? slogan
          )
        {
            CompanyName = companyName;
            About = about;
            Mission = mission;
            Vision = vision;
            Slogan = slogan;
           
            CopyrightText = $"{companyName}. © {DateTime.Now.Year}. All Rights Reserved";
            AddDomainEvent(new CompanyInfoUpdatedEvent(this.Id));
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
            AddDomainEvent(new CompanyInfoUpdatedEvent(this.Id));
        }

        public void UpdateSocialMedia(SocialMediaLinks? socialMedia)
        {
            SocialMedia = socialMedia;
        }
        public void UpdateStatistics(
        int yearsInBusiness,
        int totalClients,
        int projectsCompleted,
        int teamMembers)
        {
            if (yearsInBusiness < 0) throw new ArgumentException("Years in business cannot be negative");
            if (totalClients < 0) throw new ArgumentException("Total clients cannot be negative");
            if (projectsCompleted < 0) throw new ArgumentException("Projects completed cannot be negative");
            if (teamMembers < 0) throw new ArgumentException("Team members cannot be negative");

            YearsInBusiness = yearsInBusiness;
            TotalClients = totalClients;
            ProjectsCompleted = projectsCompleted;
            TeamMembers = teamMembers;
            AddDomainEvent(new CompanyInfoUpdatedEvent(this.Id));
        }
        public void UpdateSectionImages(
        string? missionImagePath,
        string? visionImagePath
        )
        {
            MissionImagePath = missionImagePath;
            VisionImagePath = visionImagePath;
         
        }
    }
}