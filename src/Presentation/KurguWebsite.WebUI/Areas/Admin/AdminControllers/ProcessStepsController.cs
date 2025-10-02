using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Features.ProcessSteps.Commands;
using KurguWebsite.Application.Features.ProcessSteps.Queries;
using KurguWebsite.WebUI.Areas.Admin.Controllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.ProcessSteps;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.AdminControllers
{
    public class ProcessStepsController : BaseAdminController
    {
        public ProcessStepsController(
           IMediator mediator,
           ILogger<ProcessStepsController> logger,
           IPermissionService permissionService)
           : base(mediator, logger, permissionService)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            GetBreadcrumbs(("Process Steps", null));

            var query = new GetAllProcessStepsQuery();
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Failed to load process steps");
                return View(new List<ProcessStepViewModel>());
            }

            var viewModel = result.Data.Select(dto => new ProcessStepViewModel
            {
                Id = dto.Id,
                StepNumber = dto.StepNumber,
                Title = dto.Title,
                Description = dto.Description,
                IconClass = dto.IconClass,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            GetBreadcrumbs(
                ("Process Steps", Url.Action("Index")),
                ("Create", null)
            );

            return View(new ProcessStepCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProcessStepCreateViewModel model)
        {
            if (!ValidateModel(model))
            {
                return View(model);
            }

            var command = new CreateProcessStepCommand
            {
                StepNumber = model.StepNumber,
                Title = model.Title,
                Description = model.Description,
                IconClass = model.IconClass,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Process step created successfully",
                nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteProcessStepCommand { Id = id };
            var result = await Mediator.Send(command);

            return HandleControlResult(result,
                "Process step deleted successfully",
                nameof(Index));
        }
    }
}