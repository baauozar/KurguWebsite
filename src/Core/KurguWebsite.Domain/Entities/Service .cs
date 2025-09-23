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
    public class Service : AuditableEntity, IAggregateRoot, ISeoEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string ShortDescription { get; private set; } = string.Empty;
        public string? FullDescription { get; private set; }
        public string IconPath { get; private set; } = string.Empty;
        public string? IconClass { get; private set; }
        public ServiceCategory Category { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFeatured { get; private set; }

        // SEO Properties
        public string? MetaTitle { get; private set; }
        public string? MetaDescription { get; private set; }
        public string? MetaKeywords { get; private set; }

        // Navigation Properties
        private readonly List<ServiceFeature> _features = new();
        public IReadOnlyCollection<ServiceFeature> Features => _features.AsReadOnly();

        private readonly List<CaseStudy> _caseStudies = new();
        public IReadOnlyCollection<CaseStudy> CaseStudies => _caseStudies.AsReadOnly();

        private Service() { }

        public static Service Create(
            string title,
            string description,
            string shortDescription,
            string iconPath,
            ServiceCategory category)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Service title is required", nameof(title));

            var service = new Service
            {
                Title = title,
                Slug = GenerateSlug(title),
                Description = description,
                ShortDescription = shortDescription,
                IconPath = iconPath,
                Category = category,
                IsActive = true,
                DisplayOrder = 0
            };
            service.AddDomainEvent(new ServiceCreatedEvent(service.Id));
            return service;
        }

        public void Update(
            string title,
            string description,
            string shortDescription,
            string? fullDescription,
            string iconPath,
            ServiceCategory category)
        {
            Title = title;
            Slug = GenerateSlug(title);
            Description = description;
            ShortDescription = shortDescription;
            FullDescription = fullDescription;
            IconPath = iconPath;
            Category = category;
            AddDomainEvent(new ServiceUpdatedEvent(this.Id));
        }

        public void UpdateSeo(string? metaTitle, string? metaDescription, string? metaKeywords)
        {
            MetaTitle = metaTitle;
            MetaDescription = metaDescription;
            MetaKeywords = metaKeywords;
        }

        public void SetDisplayOrder(int order)
        {
            DisplayOrder = order;
        }

        public void SetFeatured(bool isFeatured)
        {
            IsFeatured = isFeatured;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        
    
        public void AddFeature(ServiceFeature feature)
        {
            if (feature != null && !_features.Contains(feature))
                _features.Add(feature);
        }

        public void RemoveFeature(ServiceFeature feature)
        {
            _features.Remove(feature);
        }

        private static string GenerateSlug(string title)
        {
            return title.ToLower()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("/", "-")
                .Replace("\\", "-");
        }
    }
}

