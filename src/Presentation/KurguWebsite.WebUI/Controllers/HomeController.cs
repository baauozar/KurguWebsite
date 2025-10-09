using KurguWebsite.Application.Features.CaseStudies.Queries;
using KurguWebsite.Application.Features.Partners.Queries;
using KurguWebsite.Application.Features.Services.Queries;
using KurguWebsite.Application.Features.Testimonials.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var featuredServicesResult = await _mediator.Send(new GetFeaturedServicesQuery());
            var testimonialsResult = await _mediator.Send(new GetActiveTestimonialsQuery());
            var partnersResult = await _mediator.Send(new GetPartnersByTypeQuery { Type = Domain.Enums.PartnerType.TechnologyPartner });
            var caseStudiesQuery = new GetRecentCaseStudiesQuery { Count = 3 };
            var caseStudiesResult = await _mediator.Send(caseStudiesQuery);

        

            return View();
        }
    }
}