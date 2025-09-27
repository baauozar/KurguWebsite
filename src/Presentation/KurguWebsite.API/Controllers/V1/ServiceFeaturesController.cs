using Asp.Versioning;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Features.Services.Commands;
using KurguWebsite.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]

    public class ServiceFeaturesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ServiceFeaturesController(IMediator mediator)
        {
            _mediator = mediator;
        }

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
        [HttpGet]
        [ProducesResponseType(typeof(List<ServiceFeatureDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllServiceFeaturesQuery(), ct);
            
            return Ok(result.Data);
        }
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PaginatedList<ServiceFeatureDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? serviceId, [FromQuery] PaginationParams paging, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetServiceFeaturesPagedQuery
            {
                ServiceId = serviceId,
                Paging = paging
            }, ct);

         
          
            return Ok(result.Data);
        }
    }
}