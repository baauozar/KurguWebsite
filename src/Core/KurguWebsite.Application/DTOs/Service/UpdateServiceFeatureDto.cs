using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Service
{
    public class UpdateServiceFeatureDto
    {
        public required string Title { get; set; }
        public required  string Description { get; set; }
        public string? IconClass { get; set; }
    }
}
