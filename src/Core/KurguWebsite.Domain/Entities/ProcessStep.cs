using KurguWebsite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Entities
{
    public class ProcessStep : AuditableEntity, IActivatable, IOrderable
    {
     
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string? IconClass { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; }

        private ProcessStep() { }

        public static ProcessStep Create(
      
       string title,
       string description,
       string iconClass = null,
       int displayOrder = 0)
        {
            return new ProcessStep
            {
              
                Title = title,
                Description = description,
                IconClass = iconClass,
                DisplayOrder = displayOrder,
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


        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
