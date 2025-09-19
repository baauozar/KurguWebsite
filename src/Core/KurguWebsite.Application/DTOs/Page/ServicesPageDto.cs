using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Service
{
    public class ServicesPageDto
    {
        public PageDto PageInfo { get; set; } = null!;
        public List<ServiceDto> Services { get; set; } = new();
        public List<ProcessStepDto> ProcessSteps { get; set; } = new();
    }
}
