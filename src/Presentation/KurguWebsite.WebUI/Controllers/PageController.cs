// src/Presentation/KurguWebsite.Web/Controllers/PagesController.cs
using KurguWebsite.Application.Features.Pages.Queries;
using KurguWebsite.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Controllers
{
    public class PagesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PagesController> _logger;

        public PagesController(IMediator mediator, ILogger<PagesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // GET: /
        [Route("")]
        [Route("home")]
        public async Task<IActionResult> Home()
        {
            try
            {
                var result = await _mediator.Send(new GetHomePageDataQuery());

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Home page data not found");
                    return NotFound();
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                return View("Error");
            }
        }

        // GET: /about-us
        [Route("about-us")]
        [Route("about")]
        public async Task<IActionResult> About()
        {
            try
            {
                var result = await _mediator.Send(new GetAboutPageDataQuery());

                if (!result.Succeeded)
                {
                    _logger.LogWarning("About page data not found");
                    return NotFound();
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading about page");
                return View("Error");
            }
        }

        // GET: /contact
        [Route("contact")]
        public async Task<IActionResult> Contact()
        {
            try
            {
                var result = await _mediator.Send(new GetContactPageDataQuery());

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Contact page data not found");
                    return NotFound();
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contact page");
                return View("Error");
            }
        }

        // GET: /services
        [Route("services")]
        public async Task<IActionResult> Services()
        {
            try
            {
                var result = await _mediator.Send(new GetServicesPageDataQuery());

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Services page data not found");
                    return NotFound();
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading services page");
                return View("Error");
            }
        }

        // GET: /services/{slug}
        [Route("services/{slug}")]
        public async Task<IActionResult> ServiceDetail(string slug)
        {
            try
            {
                var result = await _mediator.Send(new GetServiceDetailBySlugQuery { Slug = slug });

                if (!result.Succeeded)
                {
                    return NotFound();
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service detail for slug: {Slug}", slug);
                return View("Error");
            }
        }
    }
}