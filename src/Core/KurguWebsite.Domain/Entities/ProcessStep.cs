using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class ProcessStep : AuditableEntity, IActivatable
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
       string description,
       string? iconClass = null,
       int? displayOrder = null)
        {
            return new ProcessStep
            {
                StepNumber = stepNumber,
                Title = title,
                Description = description,
                IconClass = iconClass,
                DisplayOrder = displayOrder ?? stepNumber,
                IsActive = true
            };
        }

        public void Update(string title, string description, string? iconClass)
        {
            Title = title;
            Description = description;
            IconClass = iconClass;
        }
        public void SetDisplayOrder(int order)
        {
            if (order < 0)
                throw new ArgumentException("Display order cannot be negative", nameof(order));
            DisplayOrder = order;
        }

        public void SetStepNumber(int stepNumber)
        {
            if (stepNumber < 1)
                throw new ArgumentException("Step number must be positive", nameof(stepNumber));
            StepNumber = stepNumber;
        }
        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
