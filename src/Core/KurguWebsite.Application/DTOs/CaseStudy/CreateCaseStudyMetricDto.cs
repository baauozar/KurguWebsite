using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.CaseStudy
{
    public class CreateCaseStudyMetricDto
    {
        public Guid CaseStudyId { get; set; }
        public required string MetricName { get; set; }
        public required string MetricValue { get; set; }
        public string? Icon { get; set; }
    }
}
