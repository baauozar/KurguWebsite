using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.ServiceFeaturesDto
{
    public class UpdateServiceFeatureDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? IconClass { get; set; }
    }
}
