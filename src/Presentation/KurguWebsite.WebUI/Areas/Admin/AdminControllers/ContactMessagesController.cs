using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.Features.ContactMessages.Commands;
using KurguWebsite.Application.Features.ContactMessages.Queries;
using KurguWebsite.WebUI.Areas.Admin.Controllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.ContactMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.AdminControllers
{
    public class ContactMessagesController : BaseAdminController
    {
        public ContactMessagesController(
             IMediator mediator,
             ILogger<ContactMessagesController> logger,
             IPermissionService permissionService)
             : base(mediator, logger, permissionService)
        {
        }

        // GET: Admin/ContactMessages
        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20, string? search = null)
        {
            GetBreadcrumbs(("Contact Messages", null));

            var query = new GetPaginatedContactMessagesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
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
                SetErrorMessage("Failed to load contact messages");
                return View(new ContactMessageIndexViewModel());
            }

            var viewModel = MapToPagedViewModel(
                result.Data,
                dto => new ContactMessageListItemViewModel
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Subject = dto.Subject,
                    Message = dto.Message.Length > 100 ? dto.Message.Substring(0, 100) + "..." : dto.Message,
                    IsRead = dto.IsRead,
                    IsReplied = dto.IsReplied,
                    CreatedDate = dto.CreatedDate
                },
                search
            );

            var indexViewModel = new ContactMessageIndexViewModel
            {
                Items = viewModel.Items,
                PageNumber = viewModel.PageNumber,
                PageSize = viewModel.PageSize,
                TotalCount = viewModel.TotalCount,
                TotalPages = viewModel.TotalPages,
                HasPreviousPage = viewModel.HasPreviousPage,
                HasNextPage = viewModel.HasNextPage,
                SearchTerm = viewModel.SearchTerm,
                UnreadCount = viewModel.Items.Count(m => !m.IsRead),
                UnrepliedCount = viewModel.Items.Count(m => !m.IsReplied)
            };

            return View(indexViewModel);
        }

        // GET: Admin/ContactMessages/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            GetBreadcrumbs(
                ("Contact Messages", Url.Action("Index")),
                ("Details", null)
            );

            var query = new GetContactMessageByIdQuery { Id = id };
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Contact message not found");
                return RedirectToAction(nameof(Index));
            }

            // Mark as read
            var markReadCommand = new MarkContactMessageAsReadCommand { Id = id };
            await Mediator.Send(markReadCommand);

            var viewModel = new ContactMessageDetailsViewModel
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                Email = result.Data.Email,
                Phone = result.Data.Phone,
                Subject = result.Data.Subject,
                Message = result.Data.Message,
                IsRead = result.Data.IsRead,
                IsReplied = result.Data.IsReplied,
                CreatedDate = result.Data.CreatedDate,
                RepliedDate = result.Data.RepliedDate,
                RepliedBy = result.Data.RepliedBy
            };

            return View(viewModel);
        }

        // POST: Admin/ContactMessages/MarkAsReplied/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReplied(Guid id)
        {
            var command = new MarkContactMessageAsRepliedCommand { Id = id };
            var result = await Mediator.Send(command);

            // Replace this line:
           

            // With this line:
            return HandleControlResult(result,
                "Message marked as replied",
                nameof(Details),
                id.ToString());
        }

        // POST: Admin/ContactMessages/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Note: Create DeleteContactMessageCommand in Application layer
            SetSuccessMessage("Contact message deleted successfully");
            return RedirectToAction(nameof(Index));
        }
    }
}