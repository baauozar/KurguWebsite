// src/Core/KurguWebsite.Domain/Specifications/CompanyInfoSpecifications.cs
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Domain.Specifications
{
    public class ActiveCompanyInfoSpecification : BaseSpecification<CompanyInfo>
    {
        public ActiveCompanyInfoSpecification()
            : base(ci => !ci.IsDeleted)
        {
            // Usually there's only one company info record
        }
    }

    public class CompanyInfoWithDetailsSpecification : BaseSpecification<CompanyInfo>
    {
        public CompanyInfoWithDetailsSpecification()
            : base(ci => !ci.IsDeleted)
        {
            // Include related data if needed
        }
    }
}