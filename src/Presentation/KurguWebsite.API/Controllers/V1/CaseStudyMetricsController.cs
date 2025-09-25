/*using Asp.Versioning;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.Features.CaseStudies.Commands;
using KurguWebsite.Application.Features.CaseStudies.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]

    public class CaseStudyMetricsController : BaseApiController
    {
        private readonly IMediator _mediator;
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return HandleResult(await this._mediator.Send(new GetCaseStudyMetricByIdQuery { Id = id }));
        }

        [HttpGet("case-study/{caseStudyId}")]
        public async Task<IActionResult> GetByCaseStudyId(Guid caseStudyId)
        {
            return HandleResult(await this._mediator.Send(new GetCaseStudyMetricsByCaseStudyIdQuery { CaseStudyId = caseStudyId }));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCaseStudyMetricCommand command)
        {
            return HandleResult(await this._mediator.Send(command));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateCaseStudyMetricDto dto)
        {
            var command = new UpdateCaseStudyMetricCommand { Id = id, UpdateCaseStudyMetricDto = dto };
            return HandleResult(await this._mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return HandleResult(await this._mediator.Send(new DeleteCaseStudyMetricCommand { Id = id }));
        }
    }
}*/