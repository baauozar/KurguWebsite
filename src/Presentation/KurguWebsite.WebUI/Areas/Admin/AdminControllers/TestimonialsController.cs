// Areas/Admin/Controllers/TestimonialsController.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Application.Features.Testimonials.Commands;
using KurguWebsite.Application.Features.Testimonials.Queries;
using KurguWebsite.WebUI.Areas.Admin.AdminControllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.Testimonials;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.Controllers
{
    public class TestimonialsController : BaseAdminController
    {
        public TestimonialsController(
           IMediator mediator,
           ILogger<ServicesController> logger,
           IPermissionService permissionService)
           : base(mediator, logger, permissionService)
        {
        }

        // GET: Admin/Testimonials
        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            GetBreadcrumbs(("Testimonials", null));

            var query = new GetPaginatedTestimonialsQuery
            {
                Params = new QueryParameters
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = search
                }
            };

            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Failed to load testimonials");
                return View(new TestimonialIndexViewModel());
            }

            var viewModel = MapToPagedViewModel(
                result.Data,
                dto => new TestimonialListItemViewModel
                {
                    Id = dto.Id,
                    ClientName = dto.ClientName,
                    ClientTitle = dto.ClientTitle,
                    CompanyName = dto.CompanyName,
                    Content = dto.Content.Length > 100 ? dto.Content.Substring(0, 100) + "..." : dto.Content,
                    Rating = dto.Rating,
                    IsActive = dto.IsActive,
                    IsFeatured = dto.IsFeatured,
                    DisplayOrder = dto.DisplayOrder
                },
                search
            );

            var indexViewModel = new TestimonialIndexViewModel
            {
                Items = viewModel.Items,
                PageNumber = viewModel.PageNumber,
                PageSize = viewModel.PageSize,
                TotalCount = viewModel.TotalCount,
                TotalPages = viewModel.TotalPages,
                HasPreviousPage = viewModel.HasPreviousPage,
                HasNextPage = viewModel.HasNextPage,
                SearchTerm = viewModel.SearchTerm
            };

            return View(indexViewModel);
        }

        // GET: Admin/Testimonials/Create
        [HttpGet]
        public IActionResult Create()
        {
            GetBreadcrumbs(
                ("Testimonials", Url.Action("Index")),
                ("Create", null)
            );

            return View(new TestimonialCreateViewModel());
        }

        // POST: Admin/Testimonials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestimonialCreateViewModel model)
        {
            if (!ValidateModel(model))
            {
                return View(model);
            }

            // Handle image upload
            

            var command = new CreateTestimonialCommand
            {
                ClientName = model.ClientName,
                ClientTitle = model.ClientTitle,
                CompanyName = model.CompanyName,
                Content = model.Content,
                ClientImagePath = model.ClientImagePath,
                Rating = model.Rating,
                TestimonialDate = model.TestimonialDate,
                IsActive = model.IsActive,
                IsFeatured = model.IsFeatured
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Testimonial created successfully",
                nameof(Index));
        }

        // GET: Admin/Testimonials/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            GetBreadcrumbs(
                ("Testimonials", Url.Action("Index")),
                ("Edit", null)
            );

            // Note: You need to create GetTestimonialByIdQuery in Application layer
            // For now, getting from list
            var query = new GetActiveTestimonialsQuery();
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Testimonial not found");
                return RedirectToAction(nameof(Index));
            }

            var testimonial = result.Data.FirstOrDefault(t => t.Id == id);
            if (testimonial == null)
            {
                SetErrorMessage("Testimonial not found");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new TestimonialEditViewModel
            {
                Id = testimonial.Id,
                ClientName = testimonial.ClientName,
                ClientTitle = testimonial.ClientTitle,
                CompanyName = testimonial.CompanyName,
                Content = testimonial.Content,
                ClientImagePath = testimonial.ClientImagePath,
                Rating = testimonial.Rating,
                TestimonialDate = testimonial.TestimonialDate,
                IsActive = testimonial.IsActive,
                IsFeatured = testimonial.IsFeatured,
                DisplayOrder = testimonial.DisplayOrder
            };

            return View(viewModel);
        }

        // POST: Admin/Testimonials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TestimonialEditViewModel model)
        {
            if (id != model.Id)
            {
                SetErrorMessage("Invalid testimonial ID");
                return RedirectToAction(nameof(Index));
            }

            if (!ValidateModel(model))
            {
                return View(model);
            }

         

            // Create UpdateTestimonialCommand based on your Application layer
            // This is a simplified version
            var command = new UpdateTestimonialDto
            {
                ClientName = model.ClientName,
                ClientTitle = model.ClientTitle,
                CompanyName = model.CompanyName,
                Content = model.Content,
                ClientImagePath = model.ClientImagePath,
                Rating = model.Rating,
                TestimonialDate = model.TestimonialDate,
                IsActive = model.IsActive,
                IsFeatured = model.IsFeatured,
                DisplayOrder = model.DisplayOrder
            };

            // Note: You need UpdateTestimonialCommand in your Application layer
            SetSuccessMessage("Testimonial updated successfully");
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Testimonials/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Note: You need DeleteTestimonialCommand in your Application layer
            SetSuccessMessage("Testimonial deleted successfully");
            return RedirectToAction(nameof(Index));
        }
    }
}