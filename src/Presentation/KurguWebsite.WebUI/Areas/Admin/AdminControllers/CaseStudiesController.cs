// Areas/Admin/Controllers/CaseStudiesController.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Features.CaseStudies.Commands;
using KurguWebsite.Application.Features.CaseStudies.Queries;
using KurguWebsite.Application.Features.Services.Queries;
using KurguWebsite.WebUI.Areas.Admin.AdminControllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.CaseStudies;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.Controllers
{
    public class CaseStudiesController : BaseAdminController
    {
        public CaseStudiesController(
           IMediator mediator,
           ILogger<CaseStudiesController> logger,
           IPermissionService permissionService)
           : base(mediator, logger, permissionService)
        {
        }

        // GET: Admin/CaseStudies
        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? serviceId = null)
        {
            GetBreadcrumbs(("Case Studies", null));

            var query = new GetPaginatedCaseStudiesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Failed to load case studies");
                return View(new CaseStudyIndexViewModel());
            }

            // Get services for filter dropdown
            var servicesQuery = new GetAllServicesQuery();
            var servicesResult = await Mediator.Send(servicesQuery);

            var viewModel = new CaseStudyIndexViewModel
            {
                Items = result.Data.Items.Select(dto => new CaseStudyListItemViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    Slug = dto.Slug,
                    ClientName = dto.ClientName,
                    ServiceName = dto.ServiceName,
                    CompletedDate = dto.CompletedDate,
                    IsActive = dto.IsActive,
                    IsFeatured = dto.IsFeatured
                }).ToList(),
                PageNumber = result.Data.PageNumber,
                PageSize = pageSize,
                TotalCount = result.Data.TotalCount,
                TotalPages = result.Data.TotalPages,
                HasPreviousPage = result.Data.HasPreviousPage,
                HasNextPage = result.Data.HasNextPage,
                SearchTerm = search,
                Services = servicesResult.Succeeded && servicesResult.Data != null
                    ? servicesResult.Data.Select(s => new ServiceDropdownViewModel
                    {
                        Id = s.Id,
                        Title = s.Title
                    }).ToList()
                    : new List<ServiceDropdownViewModel>(),
                SelectedServiceId = serviceId
            };

            return View(viewModel);
        }

        // GET: Admin/CaseStudies/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            GetBreadcrumbs(
                ("Case Studies", Url.Action("Index")),
                ("Details", null)
            );

            var query = new GetCaseStudyByIdQuery { Id = id };
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Case study not found");
                return RedirectToAction(nameof(Index));
            }

            // Get metrics
            var metricsQuery = new GetCaseStudyMetricsByCaseStudyIdQuery { CaseStudyId = id };
            var metricsResult = await Mediator.Send(metricsQuery);

            var viewModel = new CaseStudyDetailsViewModel
            {
                Id = result.Data.Id,
                Title = result.Data.Title,
                Slug = result.Data.Slug,
                ClientName = result.Data.ClientName,
                Description = result.Data.Description,
                ImagePath = result.Data.ImagePath,
                CompletedDate = result.Data.CompletedDate,
                Industry = result.Data.Industry,
                ServiceName = result.Data.ServiceName,
                Technologies = result.Data.Technologies,
                IsActive = result.Data.IsActive,
                IsFeatured = result.Data.IsFeatured,
                Metrics = metricsResult.Succeeded && metricsResult.Data != null
                    ? metricsResult.Data.Select(m => new CaseStudyMetricViewModel
                    {
                        Id = m.Id,
                        MetricName = m.MetricName,
                        MetricValue = m.MetricValue,
                        Icon = m.Icon
                    }).ToList()
                    : new List<CaseStudyMetricViewModel>()
            };

            return View(viewModel);
        }

        // GET: Admin/CaseStudies/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            GetBreadcrumbs(
                ("Case Studies", Url.Action("Index")),
                ("Create", null)
            );

            var viewModel = new CaseStudyCreateViewModel
            {
                CompletedDate = DateTime.Now,
                AvailableServices = await GetServicesDropdown()
            };

            return View(viewModel);
        }

        // POST: Admin/CaseStudies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaseStudyCreateViewModel model)
        {
            if (!ValidateModel(model))
            {
                model.AvailableServices = await GetServicesDropdown();
                return View(model);
            }

           

            if (string.IsNullOrEmpty(model.ImagePath))
            {
                SetErrorMessage("Image is required");
                model.AvailableServices = await GetServicesDropdown();
                return View(model);
            }

            // Parse technologies
            var technologies = !string.IsNullOrWhiteSpace(model.TechnologiesText)
                ? model.TechnologiesText.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList()
                : new List<string>();

            var command = new CreateCaseStudyCommand
            {
                Title = model.Title,
                ClientName = model.ClientName,
                Description = model.Description,
                Challenge = model.Challenge,
                Solution = model.Solution,
                Result = model.Result,
                ImagePath = model.ImagePath,
                CompletedDate = model.CompletedDate,
                Industry = model.Industry,
                ServiceId = model.ServiceId,
                Technologies = technologies,
                IsFeatured = model.IsFeatured
            };

            var result = await Mediator.Send(command);

            if (result.Succeeded && result.Data != null)
            {
                // Create metrics if any
                foreach (var metric in model.Metrics.Where(m => !string.IsNullOrWhiteSpace(m.MetricName)))
                {
                    var metricCommand = new CreateCaseStudyMetricCommand
                    {
                        CaseStudyId = result.Data.Id,
                        MetricName = metric.MetricName,
                        MetricValue = metric.MetricValue,
                        Icon = metric.Icon
                    };
                    await Mediator.Send(metricCommand);
                }
            }

            return HandleResult(result,
                "Case study created successfully",
                nameof(Index));
        }

        // GET: Admin/CaseStudies/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            GetBreadcrumbs(
                ("Case Studies", Url.Action("Index")),
                ("Edit", null)
            );

            var query = new GetCaseStudyByIdQuery { Id = id };
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Case study not found");
                return RedirectToAction(nameof(Index));
            }

            // Get metrics
            var metricsQuery = new GetCaseStudyMetricsByCaseStudyIdQuery { CaseStudyId = id };
            var metricsResult = await Mediator.Send(metricsQuery);

            var viewModel = new CaseStudyEditViewModel
            {
                Id = result.Data.Id,
                Title = result.Data.Title,
                ClientName = result.Data.ClientName,
                Description = result.Data.Description,
                ImagePath = result.Data.ImagePath,
                CompletedDate = result.Data.CompletedDate,
                Industry = result.Data.Industry,
                ServiceId = result.Data.ServiceId,
                IsActive = result.Data.IsActive,
                IsFeatured = result.Data.IsFeatured,
                TechnologiesText = string.Join(", ", result.Data.Technologies),
                Technologies = result.Data.Technologies,
                AvailableServices = await GetServicesDropdown(),
                Metrics = metricsResult.Succeeded && metricsResult.Data != null
                    ? metricsResult.Data.Select(m => new CaseStudyMetricEditViewModel
                    {
                        Id = m.Id,
                        MetricName = m.MetricName,
                        MetricValue = m.MetricValue,
                        Icon = m.Icon
                    }).ToList()
                    : new List<CaseStudyMetricEditViewModel>()
            };

            return View(viewModel);
        }

        // POST: Admin/CaseStudies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CaseStudyEditViewModel model)
        {
            if (id != model.Id)
            {
                SetErrorMessage("Invalid case study ID");
                return RedirectToAction(nameof(Index));
            }

            if (!ValidateModel(model))
            {
                model.AvailableServices = await GetServicesDropdown();
                return View(model);
            }

            // Handle image upload
         

            var command = new UpdateCaseStudyCommand
            {
                Id = model.Id,
                Title = model.Title,
                ClientName = model.ClientName,
                Description = model.Description,
                Challenge = model.Challenge,
                Solution = model.Solution,
                Result = model.Result,
                ImagePath = model.ImagePath,
                CompletedDate = model.CompletedDate,
                Industry = model.Industry,
                ServiceId = model.ServiceId,
                IsActive = model.IsActive,
                IsFeatured = model.IsFeatured
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Case study updated successfully",
                nameof(Index));
        }

        // POST: Admin/CaseStudies/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteCaseStudyCommand { Id = id };
            var result = await Mediator.Send(command);

            return HandleControlResult(result,
                "Case study deleted successfully",
                nameof(Index));
        }

        #region Metrics Management

        // POST: Admin/CaseStudies/AddMetric
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMetric(CreateCaseStudyMetricCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result, "Metric added successfully");
        }

        // POST: Admin/CaseStudies/UpdateMetric
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMetric(Guid id, UpdateCaseStudyMetricCommand command)
        {
            command.Id = id;
            var result = await Mediator.Send(command);
            return HandleResult(result, "Metric updated successfully");
        }

        // POST: Admin/CaseStudies/DeleteMetric
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMetric(Guid id)
        {
            var command = new DeleteCaseStudyMetricCommand { Id = id };
            var result = await Mediator.Send(command);
            return HandleControlResult(result, "Metric deleted successfully");
        }

        #endregion

        #region Helper Methods

        private async Task<List<ServiceDropdownViewModel>> GetServicesDropdown()
        {
            var query = new GetAllServicesQuery();
            var result = await Mediator.Send(query);

            return result.Succeeded && result.Data != null
                ? result.Data.Select(s => new ServiceDropdownViewModel
                {
                    Id = s.Id,
                    Title = s.Title
                }).ToList()
                : new List<ServiceDropdownViewModel>();
        }

        #endregion
    }
}