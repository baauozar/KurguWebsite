using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Features.ContactMessages.Commands; // adjust if your namespace differs
using KurguWebsite.WebUI.UIModel.Contact;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.WebUI.Controllers
{

    [Route("contact")]
    public class ContactController : Controller
    {
        private readonly IContactAppService _contact;
        private readonly IMapper _mapper;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IContactAppService contact, IMapper mapper, ILogger<ContactController> logger)
        {
            _contact = contact;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("Index")]
        public IActionResult Index() => View(new ContactFormViewModel());

        [HttpPost("Index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var request = _mapper.Map<KurguWebsite.Application.Contracts.Contact.ContactMessageRequest>(model);
            var result = await _contact.SubmitAsync(request, HttpContext.RequestAborted);

            if (!result.Succeeded)
            {
                if (result.Errors is string[] messages && messages.Length > 0)
                {
                    foreach (var msg in messages)
                        ModelState.AddModelError(string.Empty, msg); // summary line
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message ?? "Unable to send your message right now.");
                }

                return View(model);
            }


            TempData["ContactSuccess"] = true;
            return RedirectToAction(nameof(ThankYou));
        }

        [HttpGet("thank-you")]
        public IActionResult ThankYou()
            => (TempData["ContactSuccess"] as bool? == true) ? View() : RedirectToAction(nameof(Index));
    }
}