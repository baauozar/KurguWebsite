using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public  class CaseStudyMetric : BaseEntity
    {
        public Guid CaseStudyId { get; set; }
        public string? MetricName { get; set; }
        public string? MetricValue { get; set; }
        public string? Icon { get; set; }

        public virtual CaseStudy CaseStudy { get; set; }
    }
}
