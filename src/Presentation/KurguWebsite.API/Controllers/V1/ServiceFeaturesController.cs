using Asp.Versioning;
using KurguWebsite.Application.DTOs.ServiceFeaturesDto;
using KurguWebsite.Application.Features.Services.Commands;
using KurguWebsite.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]

    public class ServiceFeaturesController : BaseApiController
    {
        private readonly IMediator _mediator;
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return HandleResult(await _mediator.Send(new GetServiceFeatureByIdQuery { Id = id }));
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetByServiceId(Guid serviceId)
        {
            return HandleResult(await _mediator.Send(new GetServiceFeaturesByServiceIdQuery { ServiceId = serviceId }));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceFeatureCommand command)
        {
            return HandleResult(await _mediator.Send(command));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateServiceFeatureDto dto)
        {
            var command = new UpdateServiceFeatureCommand { Id = id, UpdateServiceFeatureDto = dto };
            return HandleResult(await _mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return HandleResult(await _mediator.Send(new DeleteServiceFeatureCommand { Id = id }));
        }
    }
}