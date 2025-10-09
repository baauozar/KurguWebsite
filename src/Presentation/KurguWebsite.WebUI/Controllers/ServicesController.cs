using AutoMapper;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.Features.Services.Queries;
using KurguWebsite.WebUI.UIModel.Service; // ServiceCardVm
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace KurguWebsite.WebUI.Controllers
{
    [Route("services")] // base path: /services
    public class ServicesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ServicesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        // GET /services
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetPaginatedServicesQuery
            {
                // If your query expects PaginationParams (as the error shows), use this:
                Params = new PaginationParams { PageNumber = 1, PageSize = 12 }

                // If your query actually uses QueryParameters in YOUR branch, swap to:
                // Params = new QueryParameters { PageNumber = 1, PageSize = 12 }
            });

            if (!result.Succeeded || result.Data is null)
                return View(Enumerable.Empty<ServiceCardVm>());

            var vms = result.Data.Items.Select(d => _mapper.Map<ServiceCardVm>(d)).ToList();
            return View(vms);
        }

        // GET /services/{slug}
        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var dto = await _mediator.Send(new GetServiceBySlugQuery { Slug = slug });
            if (!dto.Succeeded || dto.Data is null) return NotFound();

            var vm = _mapper.Map<ServiceCardVm>(dto.Data);
            return View(vm);
        }
    }
}
