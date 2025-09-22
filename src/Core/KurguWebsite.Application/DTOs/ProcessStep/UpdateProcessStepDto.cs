using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.ProcessStep
{
    public class UpdateProcessStepDto
    {
        public int? StepNumber { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
    }
}
