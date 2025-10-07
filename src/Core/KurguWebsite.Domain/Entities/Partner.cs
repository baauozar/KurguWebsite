using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class Partner : AuditableEntity, IActivatable, IOrderable
    {
        public string Name { get; private set; } = string.Empty;
        public string LogoPath { get; private set; } = string.Empty;
        public string? WebsiteUrl { get; private set; }
        public string? Description { get; private set; }
        public PartnerType Type { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; }


        private Partner() { }

        public static Partner Create(
                string name,
            string logoPath,
            string? websiteUrl,
            string? description,
            PartnerType type)
        {
            var partner = new Partner
            {
                Name = name,
                LogoPath = logoPath,
                Type = type,
                WebsiteUrl = websiteUrl,
                IsActive = true,
                DisplayOrder = 0
            };

            partner.AddDomainEvent(new PartnerCreatedEvent(partner.Id));
            return partner;
        }

        public void Update(
            string name,
            string logoPath,
            string? websiteUrl,
            string? description,
            PartnerType type)
        {
            Name = name;
            LogoPath = logoPath;
            WebsiteUrl = websiteUrl;
            Description = description;
            Type = type;
            AddDomainEvent(new PartnerUpdatedEvent(this.Id));
        }

        // ADD MISSING METHOD
        public void SetDisplayOrder(int order)
        {
            DisplayOrder = order;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}