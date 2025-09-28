using KurguWebsite.WebUI.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IServiceApiClient _serviceApiClient;

        public ServicesController(IServiceApiClient serviceApiClient)
        {
            _serviceApiClient = serviceApiClient;
        }

        public async Task<IActionResult> Index()
        {
            // This method comes from the generic base class!
            var services = await _serviceApiClient.GetAllAsync();
            return View(services);
        }
    }
}
