using KurguWebsite.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IMediator _mediator;

        public ServicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var query = new GetAllServicesQuery();
            var services = await _mediator.Send(query);
            return View(services);
        }

        public async Task<IActionResult> Detail(string slug)
        {
            var query = new GetServiceBySlugQuery { Slug = slug };
            var service = await _mediator.Send(query);
            return View(service);
        }
    }
}