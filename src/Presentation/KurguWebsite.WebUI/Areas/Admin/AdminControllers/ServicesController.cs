// Areas/Admin/Controllers/ServicesController.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Features.Services.Commands;
using KurguWebsite.Application.Features.Services.Queries;
using KurguWebsite.Domain.Enums;
using KurguWebsite.WebUI.Areas.Admin.AdminControllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.Controllers
{
    public class ServicesController : BaseAdminController
    {
        public ServicesController(
           IMediator mediator,
           ILogger<ServicesController> logger,
           IPermissionService permissionService)
           : base(mediator, logger, permissionService)
        {
        }

        // GET: Admin/Services
        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, bool sortDesc = false)
        {
            GetBreadcrumbs(("Services", null));

            var query = new GetPaginatedServicesQuery
            {
                Params = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = search,
                    SortBy = sortBy,
                    SortDescending = sortDesc
                }
            };

            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Failed to load services");
                return View(new ServiceIndexViewModel());
            }

            var viewModel = MapToPagedViewModel<ServiceDto, ServiceListItemViewModel>(
                result.Data,
                dto => new ServiceListItemViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    Slug = dto.Slug,
                    ShortDescription = dto.ShortDescription,
                    Category = dto.Category,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = dto.IsActive,
                    IsFeatured = dto.IsFeatured,
                    CreatedDate = dto.CreatedDate
                },
                search,
                sortBy,
                sortDesc
            );

            var indexViewModel = new ServiceIndexViewModel
            {
                Items = viewModel.Items,
                PageNumber = viewModel.PageNumber,
                PageSize = viewModel.PageSize,
                TotalCount = viewModel.TotalCount,
                TotalPages = viewModel.TotalPages,
                HasPreviousPage = viewModel.HasPreviousPage,
                HasNextPage = viewModel.HasNextPage,
                SearchTerm = viewModel.SearchTerm,
                SortBy = viewModel.SortBy,
                SortDescending = viewModel.SortDescending,
                Categories = Enum.GetNames(typeof(ServiceCategory)).ToList()
            };

            return View(indexViewModel);
        }

        // GET: Admin/Services/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            GetBreadcrumbs(
                ("Services", Url.Action("Index")),
                ("Details", null)
            );

            var query = new GetServiceBySlugQuery { Slug = id.ToString() };
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Service not found");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = MapToDetailsViewModel(result.Data);
            return View(viewModel);
        }

        // GET: Admin/Services/Create
        [HttpGet]
        public IActionResult Create()
        {
            GetBreadcrumbs(
                ("Services", Url.Action("Index")),
                ("Create", null)
            );

            var viewModel = new ServiceCreateViewModel();
            return View(viewModel);
        }

        // POST: Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCreateViewModel model)
        {
            if (!ValidateModel(model))
            {
                return View(model);
            }

            

            var command = new CreateServiceCommand
            {
                Title = model.Title,
                Description = model.Description,
                ShortDescription = model.ShortDescription,
                FullDescription = model.FullDescription,
                IconPath = model.IconPath ?? string.Empty,
                IconClass = model.IconClass,
                Category = (ServiceCategory)model.Category,
                DisplayOrder = model.DisplayOrder,
                IsFeatured = model.IsFeatured,
                MetaTitle = model.MetaTitle,
                MetaDescription = model.MetaDescription,
                MetaKeywords = model.MetaKeywords,
                Features = model.Features.Select(f => new CreateServiceFeatureDto
                {
                    Title = f.Title,
                    Description = f.Description,
                    IconClass = f.IconClass,
                    DisplayOrder = f.DisplayOrder
                }).ToList()
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Service created successfully",
                nameof(Index));
        }

        // GET: Admin/Services/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            GetBreadcrumbs(
                ("Services", Url.Action("Index")),
                ("Edit", null)
            );

            var query = new GetServiceByIdQuery { Id = id };
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Service not found");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = MapToEditViewModel(result.Data);
            return View(viewModel);
        }

        // POST: Admin/Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ServiceEditViewModel model)
        {
            if (id != model.Id)
            {
                SetErrorMessage("Invalid service ID");
                return RedirectToAction(nameof(Index));
            }

            if (!ValidateModel(model))
            {
                return View(model);
            }

     
            var command = new UpdateServiceCommand
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                ShortDescription = model.ShortDescription,
                FullDescription = model.FullDescription,
                IconPath = model.IconPath,
                IconClass = model.IconClass,
                Category = (ServiceCategory)model.Category,
                DisplayOrder = model.DisplayOrder,
                IsFeatured = model.IsFeatured,
                IsActive = model.IsActive,
                MetaTitle = model.MetaTitle,
                MetaDescription = model.MetaDescription,
                MetaKeywords = model.MetaKeywords
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Service updated successfully",
                nameof(Index));
        }

        // POST: Admin/Services/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteServiceCommand { Id = id };
            var result = await Mediator.Send(command);

            return HandleControlResult(result,
                "Service deleted successfully",
                nameof(Index));
        }

        // POST: Admin/Services/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(Guid id)
        {
            var command = new RestoreServiceCommand { Id = id };
            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Service restored successfully",
                nameof(Index));
        }

        #region Helper Methods

        private ServiceDetailsViewModel MapToDetailsViewModel(ServiceDetailDto dto)
        {
            return new ServiceDetailsViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Slug = dto.Slug,
                Description = dto.Description,
                ShortDescription = dto.ShortDescription,
                FullDescription = dto.FullDescription,
                IconPath = dto.IconPath,
                IconClass = dto.IconClass,
                Category = dto.Category,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                IsFeatured = dto.IsFeatured,
                CreatedDate = dto.CreatedDate,
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                MetaKeywords = dto.MetaKeywords,
                Features = dto.Features.Select(f => new ServiceFeatureViewModel
                {
                    Id = f.Id,
                    Title = f.Title,
                    Description = f.Description,
                    IconClass = f.IconClass,
                    DisplayOrder = f.DisplayOrder
                }).ToList(),
                RelatedCaseStudies = dto.RelatedCaseStudies.Select(c => new RelatedCaseStudyViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    ClientName = c.ClientName
                }).ToList()
            };
        }

        private ServiceEditViewModel MapToEditViewModel(ServiceDto dto)
        {
            return new ServiceEditViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                ShortDescription = dto.ShortDescription,
                IconPath = dto.IconPath,
                IconClass = dto.IconClass,
                Category = (int)Enum.Parse<ServiceCategory>(dto.Category),
                DisplayOrder = dto.DisplayOrder,
                IsFeatured = dto.IsFeatured,
                IsActive = dto.IsActive
            };
        }

        #endregion
    }
}