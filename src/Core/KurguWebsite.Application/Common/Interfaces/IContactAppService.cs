using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.Contracts.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Interfaces
{
    public interface IContactAppService
    {
        Task<Result<object>> SubmitAsync(ContactMessageRequest request, CancellationToken ct = default);
    }
}
