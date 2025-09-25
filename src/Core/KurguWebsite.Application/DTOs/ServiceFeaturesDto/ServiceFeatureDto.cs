using KurguWebsite.Application.Mappings;
using System;
using System.Collections.Generic;
using KurguWebsite.Domain.Entities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.ServiceFeaturesDto
{
    public class ServiceFeatureDto : IMapFrom<ServiceFeature>
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
    }
}
