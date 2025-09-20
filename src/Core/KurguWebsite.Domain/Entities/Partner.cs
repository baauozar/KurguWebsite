using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class Partner : AuditableEntity
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
            PartnerType type)
        {
            return new Partner
            {
                Name = name,
                LogoPath = logoPath,
                Type = type,
                IsActive = true,
                DisplayOrder = 0
            };
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