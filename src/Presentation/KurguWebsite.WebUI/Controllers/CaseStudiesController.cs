// Controllers/CaseStudiesController.cs
using AutoMapper;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.Features.CaseStudies.Queries; // adjust to your actual namespace
using KurguWebsite.WebUI.UIModel.CaseStudy;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace KurguWebsite.WebUI.Controllers
{
    [Route("case-studies")]
    public class CaseStudiesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CaseStudiesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var res = await _mediator.Send(new GetPaginatedCaseStudiesQuery
            {
                Params = new PaginationParams { PageNumber = 1, PageSize = 9 }
            });

            var items = (res.Succeeded && res.Data != null) ? res.Data.Items : Enumerable.Empty<CaseStudyDto>();
            var vms = items.Select(_mapper.Map<CaseStudyCardVm>).ToList();
            return View(vms);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var dto = await _mediator.Send(new GetCaseStudyBySlugQuery { Slug = slug });
            if (!dto.Succeeded || dto.Data is null) return NotFound();
            // You can map to a dedicated detail VM if you prefer
            var vm = _mapper.Map<CaseStudyCardVm>(dto.Data);
            return View(vm);
        }
    }
}
