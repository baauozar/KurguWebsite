using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Features.Partners.Commands;
using KurguWebsite.Application.Features.Partners.Queries;
using KurguWebsite.Domain.Enums;
using KurguWebsite.WebUI.Areas.Admin.Controllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.Partners;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.AdminControllers
{
    public class PartnersController : BaseAdminController
    {
        public PartnersController(
            IMediator mediator,
            ILogger<PartnersController> logger,
            IPermissionService permissionService)
            : base(mediator, logger, permissionService)
        {
        }

        // GET: Admin/Partners
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            GetBreadcrumbs(("Partners", null));

            var query = new GetAllPartnersQuery();
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Failed to load partners");
                return View(new PartnerIndexViewModel());
            }

            var viewModel = new PartnerIndexViewModel
            {
                Items = result.Data.Select(dto => new PartnerListItemViewModel
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    LogoPath = dto.LogoPath,
                    WebsiteUrl = dto.WebsiteUrl,
                    Type = dto.Type,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = dto.IsActive
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Admin/Partners/Create
        [HttpGet]
        public IActionResult Create()
        {
            GetBreadcrumbs(
                ("Partners", Url.Action("Index")),
                ("Create", null)
            );

            return View(new PartnerCreateViewModel());
        }

        // POST: Admin/Partners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartnerCreateViewModel model)
        {
            if (!ValidateModel(model))
            {
                return View(model);
            }

            // Handle logo upload
           

            if (string.IsNullOrEmpty(model.LogoPath))
            {
                SetErrorMessage("Logo is required");
                return View(model);
            }

            var command = new CreatePartnerCommand
            {
                Name = model.Name,
                LogoPath = model.LogoPath,
                WebsiteUrl = model.WebsiteUrl,
                Description = model.Description,
                Type = (PartnerType)model.Type
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Partner created successfully",
                nameof(Index));
        }

        // GET: Admin/Partners/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            GetBreadcrumbs(
                ("Partners", Url.Action("Index")),
                ("Edit", null)
            );

            var query = new GetAllPartnersQuery();
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Partner not found");
                return RedirectToAction(nameof(Index));
            }

            var partner = result.Data.FirstOrDefault(p => p.Id == id);
            if (partner == null)
            {
                SetErrorMessage("Partner not found");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PartnerEditViewModel
            {
                Id = partner.Id,
                Name = partner.Name,
                LogoPath = partner.LogoPath,
                WebsiteUrl = partner.WebsiteUrl,
                Description = partner.Description,
                Type = (int)Enum.Parse<PartnerType>(partner.Type),
                DisplayOrder = partner.DisplayOrder,
                IsActive = partner.IsActive
            };

            return View(viewModel);
        }

        // POST: Admin/Partners/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, PartnerEditViewModel model)
        {
            if (id != model.Id)
            {
                SetErrorMessage("Invalid partner ID");
                return RedirectToAction(nameof(Index));
            }

            if (!ValidateModel(model))
            {
                return View(model);
            }

            // Handle logo upload
        

            var command = new UpdatePartnerCommand
            {
                Id = model.Id,
                Name = model.Name,
                LogoPath = model.LogoPath,
                WebsiteUrl = model.WebsiteUrl,
                Description = model.Description,
                Type = (PartnerType)model.Type,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Partner updated successfully",
                nameof(Index));
        }

        // POST: Admin/Partners/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeletePartnerCommand { Id = id };
            var result = await Mediator.Send(command);

            return HandleControlResult(result,
                "Partner deleted successfully",
                nameof(Index));
        }
    }
}