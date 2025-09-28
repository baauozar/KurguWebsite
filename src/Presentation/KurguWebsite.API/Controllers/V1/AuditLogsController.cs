using Asp.Versioning;
using KurguWebsite.Application.Features.AuditLogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")] 
    public class AuditLogsController : BaseApiController
    {
        private readonly IMediator mediat;

        public AuditLogsController(IMediator mediat)
        {
            this.mediat = mediat;
        }

        /// <summary>
        /// View Application Logs. (Admin Only)
        /// </summary>
        ///  /// <returns>The requested application logs.</returns>
        [HttpGet]

        public async Task<IActionResult> GetAuditLogs([FromQuery] GetPaginatedAuditLogsQuery query)
        {
            var result = await mediat.Send(query);
            return Ok(result);
        }
    }
}