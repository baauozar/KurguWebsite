using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Features.Pages.Commands;
using KurguWebsite.Application.Features.Pages.Queries;
using KurguWebsite.Domain.Enums;
using KurguWebsite.WebUI.Areas.Admin.Controllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.Pages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.AdminControllers
{
    public class PagesController : BaseAdminController
    {
        public PagesController(
           IMediator mediator,
           ILogger<PagesController> logger,
           IPermissionService permissionService)
           : base(mediator, logger, permissionService)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            GetBreadcrumbs(("Pages", null));

            var pages = Enum.GetValues<PageType>()
                .Select(pt => new PageListItemViewModel
                {
                    PageType = pt.ToString(),
                    DisplayName = GetPageTypeDisplayName(pt)
                }).ToList();

            return View(pages);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(PageType pageType)
        {
            GetBreadcrumbs(
                ("Pages", Url.Action("Index")),
                ("Edit", null)
            );

            var query = new GetPageByTypeQuery { PageType = pageType };
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Page not found");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PageEditViewModel
            {
                Id = result.Data.Id,
                Title = result.Data.Title,
                Content = result.Data.Content,
                PageType = pageType,
                HeroTitle = result.Data.HeroTitle,
                HeroSubtitle = result.Data.HeroSubtitle,
                HeroDescription = result.Data.HeroDescription,
                HeroBackgroundImage = result.Data.HeroBackgroundImage,
                HeroButtonText = result.Data.HeroButtonText,
                HeroButtonUrl = result.Data.HeroButtonUrl,
                MetaTitle = result.Data.MetaTitle,
                MetaDescription = result.Data.MetaDescription,
                MetaKeywords = result.Data.MetaKeywords
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, PageEditViewModel model)
        {
            if (!ValidateModel(model))
            {
                return View(model);
            }

            var command = new UpdatePageContentCommand
            {
                Id = id,
                Title = model.Title,
                Content = model.Content,
                PageType = model.PageType,
                HeroTitle = model.HeroTitle,
                HeroSubtitle = model.HeroSubtitle,
                HeroDescription = model.HeroDescription,
                HeroBackgroundImage = model.HeroBackgroundImage,
                HeroButtonText = model.HeroButtonText,
                HeroButtonUrl = model.HeroButtonUrl,
                MetaTitle = model.MetaTitle,
                MetaDescription = model.MetaDescription,
                MetaKeywords = model.MetaKeywords
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Page updated successfully",
                nameof(Index));
        }

        private string GetPageTypeDisplayName(PageType pageType)
        {
            return pageType switch
            {
                PageType.Home => "Home Page",
                PageType.About => "About Page",
                PageType.Services => "Services Page",
                PageType.Contact => "Contact Page",
                _ => pageType.ToString()
            };
        }
    }
}
