// In BaseEntity.cs
using System;
using System.Collections.Generic;
using KurguWebsite.Domain.Events;

namespace KurguWebsite.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } // Make setter public or internal
        public DateTime CreatedDate { get; set; } // Make setter public or internal
        public string? CreatedBy { get; set; } // Make setter public or internal
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }

        // Remove all logic from the constructor
        protected BaseEntity() { }

        // The domain event properties remain the same...
        private readonly List<DomainEvent> _domainEvents = new List<DomainEvent>();
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        public void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void RemoveDomainEvent(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}