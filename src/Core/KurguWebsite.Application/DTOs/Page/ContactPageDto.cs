using KurguWebsite.Application.DTOs.CompanyInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.DTOs.Page
{
    public class ContactPageDto
    {
        public PageDto PageInfo { get; set; } = null!;
        public CompanyInfoDto CompanyInfo { get; set; } = null!;
    }
}
