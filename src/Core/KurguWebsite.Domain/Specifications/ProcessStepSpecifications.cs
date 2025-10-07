// src/Core/KurguWebsite.Domain/Specifications/ProcessStepSpecifications.cs
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Domain.Specifications
{
    public class ActiveProcessStepsSpecification : BaseSpecification<ProcessStep>
    {
        public ActiveProcessStepsSpecification()
            : base(ps => ps.IsActive && !ps.IsDeleted)
        {
            ApplyOrderBy(ps => ps.DisplayOrder);
        }
    }

    public class ProcessStepsByOrderSpecification : BaseSpecification<ProcessStep>
    {
        public ProcessStepsByOrderSpecification()
            : base(ps => ps.IsActive && !ps.IsDeleted)
        {
            ApplyOrderBy(ps => ps.DisplayOrder);
        }
    }

    public class ProcessStepSearchSpecification : BaseSpecification<ProcessStep>
    {
        public ProcessStepSearchSpecification(string? searchTerm)
            : base(ps =>
                !ps.IsDeleted &&
                ps.IsActive &&
                (string.IsNullOrEmpty(searchTerm) ||
                 ps.Title.Contains(searchTerm) ||
                 ps.Description.Contains(searchTerm)))
        {
            ApplyOrderBy(ps => ps.DisplayOrder);
        }
    }
}