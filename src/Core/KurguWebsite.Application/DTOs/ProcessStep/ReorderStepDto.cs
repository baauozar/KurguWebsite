using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.ProcessStep
{
    public class ReorderStepDto
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
        public int StepNumber { get; set; }
    }
}
