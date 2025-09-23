using System;
using System.Collections.Generic;
using KurguWebsite.Domain.Events;

namespace KurguWebsite.Domain.Common
{
    public abstract class BaseEntity
    {
        // --- YOUR EXISTING AUDITING PROPERTIES (KEEP THESE) ---
        public Guid Id { get; protected set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? LastModifiedDate { get; private set; }
        public string? CreatedBy { get; private set; }
        public string? LastModifiedBy { get; private set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }

        public void SetCreatedBy(string userId)
        {
            CreatedBy = userId;
        }

        public void SetModifiedBy(string userId)
        {
            LastModifiedBy = userId;
            LastModifiedDate = DateTime.UtcNow;
        }

        // --- NEW DOMAIN EVENT PROPERTIES (ADD THESE) ---
        private readonly List<DomainEvent> _domainEvents = new List<DomainEvent>();

        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}