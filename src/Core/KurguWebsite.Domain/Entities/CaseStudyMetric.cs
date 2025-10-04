using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public  class CaseStudyMetric : AuditableEntity, IActivatable
    {
        public Guid CaseStudyId { get; set; }
        public string? MetricName { get; set; }
        public string? MetricValue { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; } = true;

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public virtual CaseStudy? CaseStudy { get; set; }
        private CaseStudyMetric() { }

        // Factory method for creating a new metric
        public static CaseStudyMetric Create(Guid caseStudyId, string metricName, string metricValue, string? icon, int displayOrder = 0)
        {
            return new CaseStudyMetric
            {
                CaseStudyId = caseStudyId,
                MetricName = metricName,
                MetricValue = metricValue,
                Icon = icon,
                DisplayOrder = displayOrder
            };
        }

        // Method for updating an existing metric
        public void Update(string metricName, string metricValue, string? icon)
        {
            MetricName = metricName;
            MetricValue = metricValue;
            Icon = icon;
        }
        public void SetDisplayOrder(int order)
        {
            DisplayOrder = order;
        }
    }
}