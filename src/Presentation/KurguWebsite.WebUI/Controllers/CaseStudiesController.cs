using AutoMapper;
using KurguWebsite.WebUI.UIModel.CaseStudy;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// Adjust to your actual query namespaces/types
using KurguWebsite.Application.Features.CaseStudies.Queries;

namespace KurguWebsite.WebUI.Controllers
{
    [Route("CaseStudies")]
    public class CaseStudiesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<CaseStudiesController> _logger;

        public CaseStudiesController(IMediator mediator, IMapper mapper, ILogger<CaseStudiesController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        // GET /case-studies  and  /case-studies/index
        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            // If you have a paged result use that; otherwise use a simple list query.
            // Example assumes a query returning List<CaseStudyDto>.
            var result = await _mediator.Send(new GetAllActiveMetricsQuery(), ct);
            if (!result.Succeeded || result.Data is null)
                return View(new List<CaseStudyCardVm>());

            var vms = _mapper.Map<List<CaseStudyCardVm>>(result.Data);
            return View(vms);
        }

        // GET /case-studies/details/{slug}
        [HttpGet("details/{slug}", Name = "CaseStudyDetail")]
        public async Task<IActionResult> Detail(string slug, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCaseStudyBySlugQuery { Slug = slug }, ct);
            if (!result.Succeeded || result.Data is null)
                return NotFound();

            var vm = _mapper.Map<CaseStudyDetailViewModel>(result.Data);
            return View(vm);
        }

        // Optional convenience: /case-studies/{slug} -> redirect to details route
        [HttpGet("{slug}")]
        public IActionResult RedirectToDetail(string slug)
            => RedirectToRoute("CaseStudyDetail", new { slug });
    }
}
