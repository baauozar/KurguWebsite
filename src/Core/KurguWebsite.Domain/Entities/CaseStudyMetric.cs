using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public  class CaseStudyMetric : AuditableEntity
    {
        public Guid CaseStudyId { get; set; }
        public string? MetricName { get; set; }
        public string? MetricValue { get; set; }
        public string? Icon { get; set; }

        public virtual CaseStudy? CaseStudy { get; set; }
        private CaseStudyMetric() { }

        // Factory method for creating a new metric
        public static CaseStudyMetric Create(Guid caseStudyId, string metricName, string metricValue, string? icon)
        {
            return new CaseStudyMetric
            {
                CaseStudyId = caseStudyId,
                MetricName = metricName,
                MetricValue = metricValue,
                Icon = icon
            };
        }

        // Method for updating an existing metric
        public void Update(string metricName, string metricValue, string? icon)
        {
            MetricName = metricName;
            MetricValue = metricValue;
            Icon = icon;
        }

    }
}