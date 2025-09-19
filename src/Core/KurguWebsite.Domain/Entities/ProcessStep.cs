using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class ProcessStep : BaseEntity
    {
        public int StepNumber { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string? IconClass { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; }

        private ProcessStep() { }

        public static ProcessStep Create(
            int stepNumber,
            string title,
            string description)
        {
            return new ProcessStep
            {
                StepNumber = stepNumber,
                Title = title,
                Description = description,
                DisplayOrder = stepNumber,
                IsActive = true
            };
        }

        public void Update(string title, string description, string? iconClass)
        {
            Title = title;
            Description = description;
            IconClass = iconClass;
        }
    }
}
