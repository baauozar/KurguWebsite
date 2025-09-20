using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class CaseStudy : AuditableEntity  // Changed from BaseEntity to AuditableEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public string ClientName { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string? Industry { get; private set; }
        public string? Challenge { get; private set; }
        public string? Solution { get; private set; }
        public string? Result { get; private set; }
        public string ImagePath { get; private set; } = string.Empty;
        public string? ThumbnailPath { get; private set; }
        public DateTime CompletedDate { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFeatured { get; private set; }
        public int DisplayOrder { get; private set; }

        // Navigation
        public Guid? ServiceId { get; private set; }
        public virtual Service? Service { get; private set; }

        // Technologies list
        private readonly List<string> _technologies = new();
        public IReadOnlyCollection<string> Technologies => _technologies.AsReadOnly();

        // Private constructor for EF
        private CaseStudy() { }

        // Factory method for creating new case study
        public static CaseStudy Create(
            string title,
            string clientName,
            string description,
            string imagePath,
            DateTime completedDate)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required", nameof(title));

            var caseStudy = new CaseStudy
            {
                Title = title,
                Slug = GenerateSlug(title),
                ClientName = clientName,
                Description = description,
                ImagePath = imagePath,
                ThumbnailPath = imagePath,
                CompletedDate = completedDate,
                IsActive = true,
                DisplayOrder = 0
            };

            return caseStudy;
        }

        // Update method
        public void Update(
            string title,
            string clientName,
            string description,
            string? challenge,
            string? solution,
            string? result)
        {
            Title = title;
            Slug = GenerateSlug(title);
            ClientName = clientName;
            Description = description;
            Challenge = challenge;
            Solution = solution;
            Result = result;
        }

        // Set service relationship
        public void SetService(Guid? serviceId)
        {
            ServiceId = serviceId;
            if (!serviceId.HasValue)
                Service = null;
        }

        // Add technology
        public void AddTechnology(string technology)
        {
            if (!string.IsNullOrWhiteSpace(technology) && !_technologies.Contains(technology))
                _technologies.Add(technology);
        }

        // Remove technology
        public void RemoveTechnology(string technology)
        {
            _technologies.Remove(technology);
        }

        // Set featured status
        public void SetFeatured(bool isFeatured)
        {
            IsFeatured = isFeatured;
        }

        // Activate/Deactivate
        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        // Set display order
        public void SetDisplayOrder(int order)
        {
            DisplayOrder = order;
        }

        // Generate slug from title
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