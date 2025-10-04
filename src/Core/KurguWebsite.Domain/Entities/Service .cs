using KurguWebsite.Domain.Common;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.Events;
using KurguWebsite.Domain.Exceptions;
using KurguWebsite.Domain.Services;
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
    ServiceCategory category,
    string? iconClass = null,
    string? fullDescription = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Service title is required");
            if (title.Length > 200)
                throw new DomainException("Service title cannot exceed 200 characters");

            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("Description is required");
            if (description.Length > 1000)
                throw new DomainException("Description cannot exceed 1000 characters");

            if (string.IsNullOrWhiteSpace(shortDescription))
                throw new DomainException("Short description is required");
            if (shortDescription.Length > 300)
                throw new DomainException("Short description cannot exceed 300 characters");

            if (string.IsNullOrWhiteSpace(iconPath))
                throw new DomainException("Icon path is required");

            if (!Enum.IsDefined(typeof(ServiceCategory), category))
                throw new DomainException("Invalid service category");

            var service = new Service
            {
                Title = title,
                Slug = SlugGenerator.Generate(title),
                Description = description,
                ShortDescription = shortDescription,
                FullDescription = fullDescription,
                IconPath = iconPath,
                IconClass = iconClass,
                Category = category,
                IsActive = true,
                DisplayOrder = 0
            };

            service.AddDomainEvent(new ServiceCreatedEvent(service.Id));
            return service;
        }

        // Updated Update method with IconClass
        public void Update(
            string title,
            string description,
            string shortDescription,
            string? fullDescription,
            string iconPath,
            string? iconClass,
            ServiceCategory category)
        {
            Title = title;
            Description = description;
            ShortDescription = shortDescription;
            FullDescription = fullDescription;
            IconPath = iconPath;
            IconClass = iconClass;
            Category = category;
            AddDomainEvent(new ServiceUpdatedEvent(this.Id));
        }

        public void UpdateSeo(string? metaTitle, string? metaDescription, string? metaKeywords)
        {
            MetaTitle = metaTitle;
            MetaDescription = metaDescription;
            MetaKeywords = metaKeywords;
        }
        public void UpdateSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug is required.", nameof(slug));
            Slug = slug;
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


        // Service.cs (inside class)
        // Service.cs
        public void AddFeature(string title, string description, string? iconClass = null, int? displayOrder = null)
        {
            var nextOrder = displayOrder ?? (_features.Count == 0 ? 0 : _features.Max(f => f.DisplayOrder) + 1);
            var feature = ServiceFeature.Create(this.Id, title, description, iconClass, nextOrder);
            feature.Service = this;
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

