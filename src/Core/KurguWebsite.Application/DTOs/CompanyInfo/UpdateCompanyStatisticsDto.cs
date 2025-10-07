using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.CompanyInfo
{
    public class UpdateCompanyStatisticsDto
    {
        public int YearsInBusiness { get; set; }
        public int TotalClients { get; set; }
        public int ProjectsCompleted { get; set; }
        public int TeamMembers { get; set; }
    }
}
