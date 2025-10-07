using KurguWebsite.Application.Features.Pages.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Controllers
{
    public class AboutController : Controller
    {
        private readonly IMediator _mediator;

        public AboutController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch the page data for the "about-us" slug
            var query = new GetPageBySlugQuery { Slug = "about-us" };
            var aboutPageData = await _mediator.Send(query);

            if (aboutPageData == null)
            {
                return NotFound(); // Or a custom error page
            }

            // Pass the DTO to the view
            return View(aboutPageData);
        }
    }
}
