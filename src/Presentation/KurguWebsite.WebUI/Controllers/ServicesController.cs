using AutoMapper;
using KurguWebsite.Application.Features.Pages.Queries;
using KurguWebsite.Application.Features.Services.Queries;
using KurguWebsite.WebUI.UIModel.Service;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Controllers
{
    [Route("Services")] // kök: /Services
    public class ServicesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public ServicesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator; _mapper = mapper;
        }
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetServicesPageDataQuery());
            return View(result.Data);
        }

        [HttpGet("servicedetails/{slug}", Name = "ServiceDetail")]
        public async Task<IActionResult> Detail(string slug)
        {
            var result = await _mediator.Send(new GetServiceDetailBySlugQuery { Slug = slug });
            if (!result.Succeeded || result.Data is null) return NotFound();

            var vm = _mapper.Map<ServiceDetailViewModel>(result.Data);
            return View(vm);
        }
    }
}